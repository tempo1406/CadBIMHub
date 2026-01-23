using System;

namespace CadBIMHub
{
    public class AuthAction
    {
        private static AuthAction _instance;
        private static readonly object _lock = new object();

        private const string VALID_USERNAME = "admin";
        private const string VALID_PASSWORD = "123456";

        public bool IsAuthenticated { get; private set; }
        public string CurrentUser { get; private set; }

        public event EventHandler<AuthenticationChangedEventArgs> AuthenticationChanged;

        private AuthAction()
        {
            IsAuthenticated = false;
            CurrentUser = string.Empty;
        }

        public static AuthAction Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AuthAction();
                        }
                    }
                }
                return _instance;
            }
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (username == VALID_USERNAME && password == VALID_PASSWORD)
            {
                IsAuthenticated = true;
                CurrentUser = username;
                OnAuthenticationChanged(true);
                return true;
            }

            return false;
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUser = string.Empty;
            OnAuthenticationChanged(false);
        }

        protected virtual void OnAuthenticationChanged(bool isAuthenticated)
        {
            AuthenticationChanged?.Invoke(this, new AuthenticationChangedEventArgs(isAuthenticated));
        }
    }

    public class AuthenticationChangedEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; }

        public AuthenticationChangedEventArgs(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }
}
