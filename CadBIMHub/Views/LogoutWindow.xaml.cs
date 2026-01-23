using System.Windows;

namespace CadBIMHub
{
    public partial class LogoutWindow : Window
    {
        public bool LogoutConfirmed { get; private set; }

        public LogoutWindow()
        {
            InitializeComponent();
            LoadCurrentUser();
            btnCancel.Focus();
        }

        private void LoadCurrentUser()
        {
            if (AuthAction.Instance.IsAuthenticated)
            {
                txtCurrentUser.Text = AuthAction.Instance.CurrentUser;
            }
            else
            {
                txtCurrentUser.Text = "Unknown";
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutConfirmed = true;
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            LogoutConfirmed = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
