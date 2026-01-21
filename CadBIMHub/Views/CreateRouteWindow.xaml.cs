using System.Windows;
using CadBIMHub.ViewModels;

namespace CadBIMHub.Views
{
    public partial class CreateRouteWindow : Window
    {
        public CreateRouteWindow()
        {
            InitializeComponent();
            var viewModel = new CreateRouteViewModel();
            viewModel.CloseAction = () => this.Close();
            DataContext = viewModel;
        }
    }
}
