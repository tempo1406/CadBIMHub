using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using CadBIMHub.Views;
using CadBIMHub.Services;

namespace CadBIMHub
{
    public class AppCommands
    {
        [CommandMethod("CADBIM_LOGIN")]
        public void Login()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (AuthenticationService.Instance.IsAuthenticated)
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
                        AuthenticationService.Instance.CurrentUser);
                    
                    RibbonService.UpdateRibbonState();
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

            if (!AuthenticationService.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBạn chưa đăng nhập!");
                return;
            }

            try
            {
                string currentUser = AuthenticationService.Instance.CurrentUser;

                LogoutWindow logoutWindow = new LogoutWindow();
                Application.ShowModalWindow(logoutWindow);

                if (logoutWindow.LogoutConfirmed)
                {
                    AuthenticationService.Instance.Logout();
                    ed.WriteMessage("\nĐã đăng xuất! Tạm biệt {0}", currentUser);
                    RibbonService.UpdateRibbonState();
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

            if (!AuthenticationService.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBạn cần đăng nhập để sử dụng chức năng này!");
                return;
            }

            try
            {
                CreateRouteWindow createRouteWindow = new CreateRouteWindow();
                Application.ShowModalWindow(createRouteWindow);
                ed.WriteMessage("\nĐóng cửa sổ tạo lộ");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi khi tạo lộ: " + ex.Message);
            }
        }

        [CommandMethod("CADBIM_CREATE_ATTRIBUTE")]
        public void CreateAttribute()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (!AuthenticationService.Instance.IsAuthenticated)
            {
                ed.WriteMessage("\nBạn cần đăng nhập để sử dụng chức năng này!");
                return;
            }

            try
            {
                CreateAttributeWindow createAttributeWindow = new CreateAttributeWindow();
                Application.ShowModalWindow(createAttributeWindow);
                ed.WriteMessage("\nĐóng cửa sổ tạo thuộc tính");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi khi tạo thuộc tính: " + ex.Message);
            }
        }
    }
}

