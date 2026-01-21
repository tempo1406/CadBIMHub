using System.Windows;

namespace CadBIMHub.Views
{
    public partial class ImportValidationWindow : Window
    {
        public ImportValidationWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.ImportValidationViewModel();
        }
    }
}
