using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

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
    }
}
