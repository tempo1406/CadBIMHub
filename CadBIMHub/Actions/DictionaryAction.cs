using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using CadBIMHub.Models;

namespace CadBIMHub.Helpers
{
    public static class DictionaryAction
    {
        private const string ROUTE_DICT_NAME = "CADBIMHUB_ROUTES";
        private const string BATCH_DICT_NAME = "CADBIMHUB_BATCHES";

        public static void SaveRoutesToDrawing(List<RouteDetailModel> routes, Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    DBDictionary routeDict;
                    if (nod.Contains(ROUTE_DICT_NAME))
                    {
                        routeDict = (DBDictionary)tr.GetObject(nod.GetAt(ROUTE_DICT_NAME), OpenMode.ForWrite);
                        
                        foreach (DBDictionaryEntry entry in routeDict)
                        {
                            DBObject obj = tr.GetObject(entry.Value, OpenMode.ForWrite);
                            obj.Erase();
                        }
                    }
                    else
                    {
                        routeDict = new DBDictionary();
                        nod.SetAt(ROUTE_DICT_NAME, routeDict);
                        tr.AddNewlyCreatedDBObject(routeDict, true);
                    }

                    int index = 0;
                    foreach (var route in routes)
                    {
                        Xrecord xrec = new Xrecord();
                        xrec.Data = new ResultBuffer(
                            new TypedValue((int)DxfCode.Text, route.RouteName ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.BatchNo ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.ItemGroup ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.ItemDescription ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.Size ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.Symbol ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, route.Quantity ?? string.Empty)
                        );

                        string key = "ROUTE_" + index.ToString("D3");
                        routeDict.SetAt(key, xrec);
                        tr.AddNewlyCreatedDBObject(xrec, true);
                        index++;
                    }

                    tr.Commit();
                }
                catch (Exception)
                {
                    tr.Abort();
                    throw;
                }
            }
        }

        public static List<RouteDetailModel> LoadRoutesFromDrawing(Database db)
        {
            List<RouteDetailModel> routes = new List<RouteDetailModel>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                    if (nod.Contains(ROUTE_DICT_NAME))
                    {
                        DBDictionary routeDict = (DBDictionary)tr.GetObject(nod.GetAt(ROUTE_DICT_NAME), OpenMode.ForRead);

                        foreach (DBDictionaryEntry entry in routeDict)
                        {
                            Xrecord xrec = (Xrecord)tr.GetObject(entry.Value, OpenMode.ForRead);
                            TypedValue[] values = xrec.Data.AsArray();

                            if (values.Length >= 7)
                            {
                                routes.Add(new RouteDetailModel
                                {
                                    RouteName = values[0].Value.ToString(),
                                    BatchNo = values[1].Value.ToString(),
                                    ItemGroup = values[2].Value.ToString(),
                                    ItemDescription = values[3].Value.ToString(),
                                    Size = values[4].Value.ToString(),
                                    Symbol = values[5].Value.ToString(),
                                    Quantity = values[6].Value.ToString()
                                });
                            }
                        }
                    }

                    tr.Commit();
                }
                catch (Exception)
                {
                    routes.Clear();
                }
            }

            return routes;
        }

        public static void SaveBatchesToDrawing(List<BatchInfoModel> batches, Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    DBDictionary batchDict;
                    if (nod.Contains(BATCH_DICT_NAME))
                    {
                        batchDict = (DBDictionary)tr.GetObject(nod.GetAt(BATCH_DICT_NAME), OpenMode.ForWrite);
                        
                        foreach (DBDictionaryEntry entry in batchDict)
                        {
                            DBObject obj = tr.GetObject(entry.Value, OpenMode.ForWrite);
                            obj.Erase();
                        }
                    }
                    else
                    {
                        batchDict = new DBDictionary();
                        nod.SetAt(BATCH_DICT_NAME, batchDict);
                        tr.AddNewlyCreatedDBObject(batchDict, true);
                    }

                    int index = 0;
                    foreach (var batch in batches)
                    {
                        Xrecord xrec = new Xrecord();
                        xrec.Data = new ResultBuffer(
                            new TypedValue((int)DxfCode.Text, batch.BatchCode ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, batch.InstallationCondition ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, batch.InstallationSpace ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, batch.WorkPackage ?? string.Empty),
                            new TypedValue((int)DxfCode.Text, batch.Phase ?? string.Empty)
                        );

                        string key = batch.BatchCode ?? ("BATCH_" + index.ToString("D3"));
                        batchDict.SetAt(key, xrec);
                        tr.AddNewlyCreatedDBObject(xrec, true);
                        index++;
                    }

                    tr.Commit();
                }
                catch (Exception)
                {
                    tr.Abort();
                    throw;
                }
            }
        }

        public static List<BatchInfoModel> LoadBatchesFromDrawing(Database db)
        {
            List<BatchInfoModel> batches = new List<BatchInfoModel>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                    if (nod.Contains(BATCH_DICT_NAME))
                    {
                        DBDictionary batchDict = (DBDictionary)tr.GetObject(nod.GetAt(BATCH_DICT_NAME), OpenMode.ForRead);

                        foreach (DBDictionaryEntry entry in batchDict)
                        {
                            Xrecord xrec = (Xrecord)tr.GetObject(entry.Value, OpenMode.ForRead);
                            TypedValue[] values = xrec.Data.AsArray();

                            if (values.Length >= 5)
                            {
                                batches.Add(new BatchInfoModel
                                {
                                    BatchCode = values[0].Value.ToString(),
                                    InstallationCondition = values[1].Value.ToString(),
                                    InstallationSpace = values[2].Value.ToString(),
                                    WorkPackage = values[3].Value.ToString(),
                                    Phase = values[4].Value.ToString()
                                });
                            }
                        }
                    }

                    tr.Commit();
                }
                catch (Exception)
                {
                    batches.Clear();
                }
            }

            return batches;
        }
    }
}
