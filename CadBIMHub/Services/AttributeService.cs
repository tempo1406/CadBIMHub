using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CadBIMHub.Models;

namespace CadBIMHub.Services
{
    public class AttributeService
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

        public static bool SelectPolylineOrMLineByLayer(out int count, out string layerName)
        {
            count = 0;
            layerName = string.Empty;
            _selectedObjects.Clear();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;

            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptEntityOptions entityOpts = new PromptEntityOptions("\nChọn 1 đối tượng để nhận diện layer: ");
            PromptEntityResult entityResult = ed.GetEntity(entityOpts);
            
            if (entityResult.Status != PromptStatus.OK)
                return false;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity selectedEntity = tr.GetObject(entityResult.ObjectId, OpenMode.ForRead) as Entity;
                    if (selectedEntity == null)
                        return false;

                    layerName = selectedEntity.Layer;
                    tr.Commit();
                }
                catch
                {
                    tr.Abort();
                    return false;
                }
            }

            if (string.IsNullOrEmpty(layerName))
                return false;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                    foreach (ObjectId objId in btr)
                    {
                        Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        
                        if (ent != null && ent.Layer == layerName)
                        {
                            if (ent is Polyline || ent is Polyline2d || ent is Polyline3d || ent is Mline)
                            {
                                _selectedObjects.Add(objId);
                            }
                        }
                    }

                    count = _selectedObjects.Count;
                    tr.Commit();
                }
                catch
                {
                    tr.Abort();
                    return false;
                }
            }

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
            bool isSingleObject = totalCount == 1;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId objId in _selectedObjects)
                    {
                        Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        if (ent == null) continue;

                        ObjectId oldTextId = ObjectId.Null;
                        bool hasExistingData = HasXData(ent, out oldTextId);

                        string objectType = GetObjectTypeSuffix(ent);
                        string newTextContent = CreateAttributeTextContent(routeName, attributes, routeDetails, objectType);
                        ObjectId newTextId = ObjectId.Null;

                        if (hasExistingData)
                        {
                            var result = System.Windows.MessageBox.Show(
                                "Bạn có muốn ghi đè thuộc tính không?",
                                "Ghi đè",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Question);

                            if (result != System.Windows.MessageBoxResult.Yes)
                                continue;

                            bool updated = false;
                            if (!oldTextId.IsNull && oldTextId.IsValid)
                            {
                                try
                                {
                                    DBObject oldObj = tr.GetObject(oldTextId, OpenMode.ForRead);
                                    
                                    if (!oldObj.IsErased)
                                    {
                                        oldObj.UpgradeOpen();
                                        
                                        if (oldObj is MLeader oldMLeader)
                                        {
                                            MText mtext = oldMLeader.MText;
                                            if (mtext != null)
                                            {
                                                mtext.Contents = newTextContent;
                                                oldMLeader.MText = mtext;
                                                newTextId = oldTextId;
                                                updated = true;
                                            }
                                        }
                                        else if (oldObj is MText oldMText)
                                        {
                                            oldMText.Contents = newTextContent;
                                            newTextId = oldTextId;
                                            updated = true;
                                        }
                                        
                                        oldObj.DowngradeOpen();
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    ed.WriteMessage($"\nLỗi khi cập nhật: {ex.Message}");
                                }
                            }
                            
                            if (!updated)
                            {
                                if (isSingleObject)
                                {
                                    newTextId = CreateMLeaderInteractive(tr, db, ed, newTextContent);
                                    if (newTextId.IsNull)
                                        continue;
                                }
                                else
                                {
                                    Point3d textPosition = GetAutoTextPosition(ent);
                                    newTextId = CreateAttributeText(tr, db, textPosition, routeName, attributes, routeDetails, objectType);
                                }
                            }
                        }
                        else
                        {
                            if (isSingleObject)
                            {
                                newTextId = CreateMLeaderInteractive(tr, db, ed, newTextContent);
                                if (newTextId.IsNull)
                                    continue;
                            }
                            else
                            {
                                Point3d textPosition = GetAutoTextPosition(ent);
                                newTextId = CreateAttributeText(tr, db, textPosition, routeName, attributes, routeDetails, objectType);
                            }
                        }

                        // Cập nhật XData nếu có textId hợp lệ
                        if (!newTextId.IsNull)
                        {
                            ent.UpgradeOpen();
                            SetXData(ent, routeName, attributes, routeDetails, newTextId);
                            ent.DowngradeOpen();

                            successCount++;
                            onProgress?.Invoke(successCount, totalCount);
                        }
                    }

                    tr.Commit();
                    
                    if (isSingleObject)
                    {
                        ed.WriteMessage($"\nĐã gán thuộc tính cho {successCount} đối tượng (có MText + MLeader)");
                    }
                    else
                    {
                        ed.WriteMessage($"\nĐã gán thuộc tính cho {successCount}/{totalCount} đối tượng (có MText tự động)");
                    }
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
            
            string handleString = textId.IsNull ? string.Empty : textId.Handle.ToString();
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, handleString));

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
            return ent is Mline ? "/1" : "/0";
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

            string textContent = CreateAttributeTextContent(routeName, attributes, routeDetails, objectTypeSuffix);

            MText mtext = new MText();
            mtext.Location = position;
            mtext.Contents = textContent;
            mtext.TextHeight = 2.5;
            mtext.Attachment = AttachmentPoint.MiddleLeft;

            ObjectId mtextId = btr.AppendEntity(mtext);
            tr.AddNewlyCreatedDBObject(mtext, true);
            
            return mtextId;
        }

        private static string CreateAttributeTextContent(
            string routeName, 
            List<AttributeDetailModel> attributes,
            List<RouteDetailModel> routeDetails,
            string objectTypeSuffix)
        {
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
            return $"{routeName}-{qtySymbolString}-{lastSize}{objectTypeSuffix}";
        }

        private static ObjectId CreateMLeaderInteractive(Transaction tr, Database db, Editor ed, string textContent)
        {
            List<Point3d> leaderPoints = new List<Point3d>();
            
            PromptPointOptions firstPointOpts = new PromptPointOptions("\nChọn điểm đầu MLeader (gần đối tượng): ");
            firstPointOpts.AllowNone = false;
            PromptPointResult firstResult = ed.GetPoint(firstPointOpts);

            if (firstResult.Status != PromptStatus.OK)
                return ObjectId.Null;

            leaderPoints.Add(firstResult.Value);

            while (true)
            {
                PromptPointOptions nextPointOpts = new PromptPointOptions(
                    leaderPoints.Count == 1 
                        ? "\nChọn điểm tiếp theo hoặc Enter để kết thúc: " 
                        : "\nChọn điểm tiếp theo hoặc Enter để kết thúc: ");
                nextPointOpts.AllowNone = true;
                nextPointOpts.UseBasePoint = true;
                nextPointOpts.BasePoint = leaderPoints[leaderPoints.Count - 1];
                
                PromptPointResult nextResult = ed.GetPoint(nextPointOpts);

                if (nextResult.Status == PromptStatus.OK)
                {
                    leaderPoints.Add(nextResult.Value);
                }
                else
                {
                    break;
                }
            }

            if (leaderPoints.Count < 2)
            {
                ed.WriteMessage("\nCần ít nhất 2 điểm để tạo MLeader!");
                return ObjectId.Null;
            }

            return CreateMLeaderWithContent(tr, db, leaderPoints, textContent);
        }

        private static ObjectId CreateMLeaderWithContent(Transaction tr, Database db, List<Point3d> points, string textContent)
        {
            try
            {
                if (points == null || points.Count < 2)
                    return ObjectId.Null;

                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                MLeader mleader = new MLeader();
                mleader.SetDatabaseDefaults(db);

                int leaderIndex = mleader.AddLeader();
                int leaderLineIndex = mleader.AddLeaderLine(leaderIndex);

                for (int i = 0; i < points.Count; i++)
                {
                    if (i == 0)
                    {
                        mleader.AddFirstVertex(leaderLineIndex, points[i]);
                    }
                    else
                    {
                        mleader.AddLastVertex(leaderLineIndex, points[i]);
                    }
                }

                mleader.ContentType = ContentType.MTextContent;
                
                MText mtext = new MText();
                mtext.SetDatabaseDefaults(db);
                mtext.Contents = textContent;
                mtext.TextHeight = 2.5;
                mtext.Attachment = AttachmentPoint.MiddleLeft;
                mleader.MText = mtext;

                mleader.ArrowSymbolId = ObjectId.Null;
                mleader.EnableLanding = false;
                mleader.EnableDogleg = false;

                ObjectId mleaderId = btr.AppendEntity(mleader);
                tr.AddNewlyCreatedDBObject(mleader, true);
                
                return mleaderId;
            }
            catch (System.Exception ex)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage($"\nLỗi tạo MLeader với content: {ex.Message}");
                }
                return ObjectId.Null;
            }
        }

        private static Point3d GetAutoTextPosition(Entity ent)
        {
            try
            {
                Extents3d ext = ent.GeometricExtents;
                double offsetX = (ext.MaxPoint.X - ext.MinPoint.X) * 0.1;
                double offsetY = (ext.MaxPoint.Y - ext.MinPoint.Y) * 0.1;
                
                Point3d autoPosition = new Point3d(
                    ext.MaxPoint.X + offsetX,
                    ext.MaxPoint.Y + offsetY,
                    ext.MaxPoint.Z
                );
                
                return autoPosition;
            }
            catch
            {
                return Point3d.Origin;
            }
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
