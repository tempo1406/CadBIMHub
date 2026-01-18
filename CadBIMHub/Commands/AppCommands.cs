using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using CadBIMHub.Views;

namespace CadBIMHub
{
    public class AppCommands
    {
        [CommandMethod("CADBIM_LOGIN")]
        public void Login()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (AuthenticationManager.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBạn đã đăng nhập rồi!");
                return;
            }

            try
            {
                LoginWindow loginWindow = new LoginWindow();
                Application.ShowModalWindow(loginWindow);

                if (loginWindow.LoginSuccess)
                {
                    ed.WriteMessage("\nĐăng nhập thành công! Chào mừng {0}", 
                        AuthenticationManager.Instance.CurrentUser);
                    
                    RibbonStateManager.UpdateRibbonState();
                }
                else
                {
                    ed.WriteMessage("\nĐăng nhập bị hủy");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi khi đăng nhập: " + ex.Message);
            }
        }

        [CommandMethod("CADBIM_LOGOUT")]
        public void Logout()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (!AuthenticationManager.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBạn chưa đăng nhập!");
                return;
            }

            try
            {
                string currentUser = AuthenticationManager.Instance.CurrentUser;

                LogoutWindow logoutWindow = new LogoutWindow();
                Application.ShowModalWindow(logoutWindow);

                if (logoutWindow.LogoutConfirmed)
                {
                    AuthenticationManager.Instance.Logout();
                    ed.WriteMessage("\nĐã đăng xuất! Tạm biệt {0}", currentUser);
                    RibbonStateManager.UpdateRibbonState();
                }
                else
                {
                    ed.WriteMessage("\nHủy đăng xuất");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi khi đăng xuất" + ex.Message);
            }
        }

        [CommandMethod("CADBIM_CREATE_ROUTE")]
        public void CreateRoute()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (!AuthenticationManager.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBan can dang nhap de su dung chuc nang nay!");
                return;
            }

            try
            {
                CreateRouteWindow createRouteWindow = new CreateRouteWindow();
                Application.ShowModalWindow(createRouteWindow);
                ed.WriteMessage("\nDong cua so Tao lo");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLoi khi tao lo: " + ex.Message);
            }
        }

        [CommandMethod("CADBIM_LOAD_DATA")]
        public void LoadData()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                var routes = Helpers.DictionaryManager.LoadRoutesFromDrawing(doc.Database);
                var batches = Helpers.DictionaryManager.LoadBatchesFromDrawing(doc.Database);

                ed.WriteMessage("\n=== CADBIMHUB DATA ===");
                ed.WriteMessage("\nSo luong Routes: {0}", routes.Count);
                ed.WriteMessage("\nSo luong Batches: {0}", batches.Count);

                if (routes.Count > 0)
                {
                    ed.WriteMessage("\n\n--- ROUTES ---");
                    foreach (var route in routes)
                    {
                        ed.WriteMessage("\n{0} | {1} | {2} | {3} | {4} | {5} | {6}",
                            route.RouteName, route.BatchNo, route.ItemGroup, route.ItemDescription,
                            route.Size, route.Symbol, route.Quantity);
                    }
                }

                if (batches.Count > 0)
                {
                    ed.WriteMessage("\n\n--- BATCHES ---");
                    foreach (var batch in batches)
                    {
                        ed.WriteMessage("\n{0} | {1} | {2} | {3} | {4}",
                            batch.BatchCode, batch.InstallationCondition, batch.InstallationSpace,
                            batch.WorkPackage, batch.Phase);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLoi khi load data: " + ex.Message);
            }
        }
    }
}

