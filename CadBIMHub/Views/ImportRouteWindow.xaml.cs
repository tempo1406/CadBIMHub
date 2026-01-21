using System;
using System.Windows;
using System.Windows.Input;

namespace CadBIMHub.Views
{
    public partial class ImportRouteWindow : Window
    {
        public ImportRouteWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.ImportRouteViewModel();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    var viewModel = DataContext as ViewModels.ImportRouteViewModel;
                    viewModel?.HandleFileDrop(files[0]);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            Window_Drop(sender, e);
        }

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            Window_DragOver(sender, e);
        }

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as ViewModels.ImportRouteViewModel;
            viewModel?.BrowseFile();
        }
    }
}
