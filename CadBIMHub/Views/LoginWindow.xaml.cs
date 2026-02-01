using System;
using System.Windows;
using System.Windows.Input;
using CadBIMHub.Services;

namespace CadBIMHub
{
    public partial class LoginWindow : Window
    {
        public bool LoginSuccess { get; private set; }
        private bool isPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();
            txtPassword.KeyDown += TxtPassword_KeyDown;
            txtPasswordVisible.KeyDown += TxtPassword_KeyDown;
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                iconEye.Text = "\uF78D";
                txtPasswordVisible.Focus();
                txtPasswordVisible.SelectionStart = txtPasswordVisible.Text.Length;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                iconEye.Text = "\uED1A";
                txtPassword.Focus();
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible && txtPassword.Visibility == Visibility.Visible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isPasswordVisible && txtPasswordVisible.Visibility == Visibility.Visible)
            {
                txtPassword.Password = txtPasswordVisible.Text;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = isPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Đăng nhập thất bại!\nVui lòng nhập đầy đủ tên và mật khẩu",
                    "Lỗi đăng nhập",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            bool success = AuthenticationService.Instance.Login(username, password);

            if (success)
            {
                LoginSuccess = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Đăng nhập thất bại!\nTên người dùng hoặc mật khẩu không chính xác.",
                    "Lỗi đăng nhập",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                txtPassword.Password = string.Empty;
                txtPasswordVisible.Text = string.Empty;
                
                if (isPasswordVisible)
                    txtPasswordVisible.Focus();
                else
                    txtPassword.Focus();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            LoginSuccess = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
