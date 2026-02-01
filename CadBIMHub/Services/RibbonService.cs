using Autodesk.Windows;

namespace CadBIMHub.Services
{
    public static class RibbonService
    {
        private const string LOGIN_BUTTON_ID = "CADBIM_LOGIN";
        private const string LOGOUT_BUTTON_ID = "CADBIM_LOGOUT";

        private static readonly string[] PROTECTED_BUTTONS = new string[]
        {
             "CADBIM_SETTING",
             "CADBIM_LOADSPEC",
             "CADBIM_CHECKSPEC",
             "CADBIM_CREATE_ROUTE",
             "CADBIM_CREATE_ATTRIBUTE",
             "CADBIM_ASSIGNSPEC",
             "CADBIM_SETTING_ASSEMBLY",
        };

        static RibbonService()
        {
            AuthenticationService.Instance.AuthenticationChanged += OnAuthenticationChanged;
        }

        private static void OnAuthenticationChanged(object sender, AuthenticationChangedEventArgs e)
        {
            UpdateRibbonState();
        }

        public static void UpdateRibbonState()
        {
            bool isAuthenticated = AuthenticationService.Instance.IsAuthenticated;

            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            UpdateButtonState(LOGIN_BUTTON_ID, !isAuthenticated);

            UpdateButtonState(LOGOUT_BUTTON_ID, isAuthenticated);

            foreach (string buttonId in PROTECTED_BUTTONS)
            {
                UpdateButtonState(buttonId, isAuthenticated);
            }
        }

        private static void UpdateButtonState(string buttonId, bool enabled)
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            foreach (var tab in ribbon.Tabs)
            {
                var button = FindRibbonButton(tab, buttonId);
                if (button != null)
                {
                    button.IsEnabled = enabled;
                    return;
                }
            }
        }

        private static RibbonButton FindRibbonButton(RibbonTab tab, string buttonId)
        {
            foreach (var panel in tab.Panels)
            {
                var button = FindButtonInPanel(panel, buttonId);
                if (button != null)
                    return button;
            }
            return null;
        }
        private static RibbonButton FindButtonInPanel(RibbonPanel panel, string buttonId)
        {
            foreach (var item in panel.Source.Items)
            {
                if (item is RibbonButton button && button.Id == buttonId)
                {
                    return button;
                }
                else if (item is RibbonRowPanel rowPanel)
                {
                    foreach (var rowItem in rowPanel.Items)
                    {
                        if (rowItem is RibbonButton rowButton && rowButton.Id == buttonId)
                        {
                            return rowButton;
                        }
                    }
                }
            }
            return null;
        }

        public static void Initialize()
        {
            UpdateRibbonState();
        }
    }
}
