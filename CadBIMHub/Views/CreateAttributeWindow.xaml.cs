using System.Windows;
using CadBIMHub.ViewModels;

namespace CadBIMHub.Views
{
    public partial class CreateAttributeWindow : Window
    {
        public CreateAttributeWindow()
        {
            InitializeComponent();
            var viewModel = new CreateAttributeViewModel();
            viewModel.CloseAction = () => this.Close();
            DataContext = viewModel;
        }
    }
}
