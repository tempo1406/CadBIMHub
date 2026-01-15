using System.Windows;
using System.Windows.Controls;
using CadBIMHub.ViewModels;

namespace CadBIMHub.Views
{
    public partial class CreateRouteWindow : Window
    {
        public CreateRouteWindow()
        {
            InitializeComponent();
        }

        private void dgRouteDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CreateRouteViewModel viewModel)
            {
                viewModel.UpdateSelectedCount();
            }
        }
    }
}
