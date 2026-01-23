using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CadBIMHub.Models;

namespace CadBIMHub.Helpers
{
    public class AssignAttributeAction
    {
        private const string XDATA_APP_NAME = "CADBIMHUB_ATTRIBUTES";
        private static List<ObjectId> _selectedObjects = new List<ObjectId>();

        public static bool SelectPolylineOrMLine(out int count)
        {
            count = 0;
            _selectedObjects.Clear();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;

            Editor ed = doc.Editor;

            PromptSelectionOptions selOpts = new PromptSelectionOptions();
            selOpts.MessageForAdding = "\nChọn các đối tượng Polyline hoặc MLine: ";
            selOpts.AllowDuplicates = false;

            TypedValue[] filterList = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, "<OR"),
                new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int)DxfCode.Start, "POLYLINE"),
                new TypedValue((int)DxfCode.Start, "MLINE"),
                new TypedValue((int)DxfCode.Operator, "OR>")
            };

            SelectionFilter filter = new SelectionFilter(filterList);
            PromptSelectionResult selResult = ed.GetSelection(selOpts, filter);

            if (selResult.Status != PromptStatus.OK)
                return false;

            _selectedObjects = selResult.Value.GetObjectIds().ToList();
            count = _selectedObjects.Count;

            return count > 0;
        }

        public static void AssignAttributes(
            string routeName, 
            List<AttributeDetailModel> attributes,
            List<RouteDetailModel> routeDetails,
            Action<int, int> onProgress = null)
        {
            if (_selectedObjects == null || _selectedObjects.Count == 0)
            {
                throw new Exception("Chưa chọn đối tượng nào!");
            }

            if (attributes == null || attributes.Count == 0)
            {
                throw new Exception("Danh sách thuộc tính trống!");
            }

            if (routeDetails == null || routeDetails.Count == 0)
            {
                throw new Exception("Không tìm thấy thông tin route!");
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) throw new Exception("Không tìm thấy Document AutoCAD");

            Database db = doc.Database;
            Editor ed = doc.Editor;

            RegisterXDataApp(db);

            int successCount = 0;
            int totalCount = _selectedObjects.Count;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId objId in _selectedObjects)
                    {
                        Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        if (ent == null) continue;

                        ObjectId oldTextId = ObjectId.Null;
                        Point3d textPosition = Point3d.Origin;
                        bool hasExistingData = HasXData(ent, out oldTextId);

                        if (hasExistingData)
                        {
                            var result = System.Windows.MessageBox.Show(
                                $"Đối tượng {ent.GetType().Name} đã có thuộc tính!\n\nBạn có muốn ghi đè thuộc tính không?",
                                "Xác nhận ghi đè",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Question);

                            if (result != System.Windows.MessageBoxResult.Yes)
                                continue;

                            if (!oldTextId.IsNull && oldTextId.IsValid)
                            {
                                try
                                {
                                    DBObject oldObj = tr.GetObject(oldTextId, OpenMode.ForRead);
                                    if (oldObj is MText oldMText)
                                    {
                                        textPosition = oldMText.Location;
                                        oldMText.UpgradeOpen();
                                        oldMText.Erase();
                                    }
                                }
                                catch
                                {
                                    textPosition = Point3d.Origin;
                                }
                            }
                        }

                        if (textPosition == Point3d.Origin)
                        {
                            PromptPointOptions pointOpts = new PromptPointOptions("\nChọn điểm để đặt Text hiển thị thuộc tính: ");
                            pointOpts.AllowNone = true;
                            PromptPointResult pointResult = ed.GetPoint(pointOpts);

                            if (pointResult.Status != PromptStatus.OK)
                                continue;

                            textPosition = pointResult.Value;
                        }

                        ent.UpgradeOpen();

                        string objectType = GetObjectTypeSuffix(ent);
                        ObjectId newTextId = CreateAttributeText(tr, db, textPosition, routeName, attributes, routeDetails, objectType);

                        SetXData(ent, routeName, attributes, routeDetails, newTextId);

                        ent.DowngradeOpen();

                        successCount++;
                        onProgress?.Invoke(successCount, totalCount);
                    }

                    tr.Commit();
                    ed.WriteMessage($"\nĐã gán thuộc tính cho {successCount}/{totalCount} đối tượng");
                }
                catch (Exception)
                {
                    tr.Abort();
                    throw;
                }
            }

            _selectedObjects.Clear();
        }

        private static void RegisterXDataApp(Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                RegAppTable regAppTable = tr.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;

                if (!regAppTable.Has(XDATA_APP_NAME))
                {
                    regAppTable.UpgradeOpen();
                    RegAppTableRecord regApp = new RegAppTableRecord();
                    regApp.Name = XDATA_APP_NAME;
                    regAppTable.Add(regApp);
                    tr.AddNewlyCreatedDBObject(regApp, true);
                }

                tr.Commit();
            }
        }

        private static bool HasXData(Entity ent, out ObjectId textId)
        {
            textId = ObjectId.Null;
            ResultBuffer xdata = ent.GetXDataForApplication(XDATA_APP_NAME);
            
            if (xdata == null)
                return false;

            try
            {
                TypedValue[] values = xdata.AsArray();
                if (values.Length > 3)
                {
                    string handleString = values[3].Value.ToString();
                    
                    if (!string.IsNullOrEmpty(handleString))
                    {
                        long handleValue = Convert.ToInt64(handleString, 16);
                        Handle handle = new Handle(handleValue);
                        
                        Database db = ent.Database;
                        if (db.TryGetObjectId(handle, out ObjectId objId))
                        {
                            textId = objId;
                        }
                    }
                }
            }
            catch
            {
                textId = ObjectId.Null;
            }

            return true;
        }

        private static void SetXData(Entity ent, string routeName, List<AttributeDetailModel> attributes, List<RouteDetailModel> routeDetails, ObjectId textId)
        {
            ResultBuffer rb = new ResultBuffer();
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, XDATA_APP_NAME));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, routeName));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, (short)attributes.Count));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, textId.Handle.ToString()));

            foreach (var attr in attributes)
            {
                var routeDetail = routeDetails.FirstOrDefault(r => 
                    r.Symbol == attr.Symbol && r.Quantity == attr.Quantity);
                
                string size = routeDetail?.Size ?? string.Empty;
                
                rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, attr.Symbol ?? string.Empty));
                rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, attr.Quantity ?? string.Empty));
                rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, size));
            }

            ent.XData = rb;
        }

        private static string GetObjectTypeSuffix(Entity ent)
        {
            if (ent is Mline)
                return "/1";
            else
                return "/0";
        }

        private static ObjectId CreateAttributeText(
            Transaction tr, 
            Database db, 
            Point3d position, 
            string routeName, 
            List<AttributeDetailModel> attributes,
            List<RouteDetailModel> routeDetails,
            string objectTypeSuffix)
        {
            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            List<string> qtySymbolParts = new List<string>();
            string lastSize = string.Empty;
            
            foreach (var attr in attributes)
            {
                var routeDetail = routeDetails.FirstOrDefault(r => 
                    r.Symbol == attr.Symbol && r.Quantity == attr.Quantity);
                
                string size = routeDetail?.Size ?? string.Empty;
                
                qtySymbolParts.Add($"{attr.Quantity}{attr.Symbol}");
                
                if (!string.IsNullOrEmpty(size))
                    lastSize = size;
            }

            string qtySymbolString = string.Join(",", qtySymbolParts);
            string textContent = $"{routeName}-{qtySymbolString}-{lastSize}{objectTypeSuffix}";

            MText mtext = new MText();
            mtext.Location = position;
            mtext.Contents = textContent;
            mtext.TextHeight = 2.5;
            mtext.Attachment = AttachmentPoint.MiddleLeft;

            ObjectId mtextId = btr.AppendEntity(mtext);
            tr.AddNewlyCreatedDBObject(mtext, true);
            
            return mtextId;
        }

        public static int GetSelectedCount()
        {
            return _selectedObjects?.Count ?? 0;
        }

        public static void ClearSelection()
        {
            _selectedObjects?.Clear();
        }
    }
}
