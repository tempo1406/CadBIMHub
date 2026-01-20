using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CadBIMHub.Helpers;
using CadBIMHub.Models;
using Microsoft.Win32;

namespace CadBIMHub.ViewModels
{
    public class ImportRouteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _selectedFilePath;
        private string _selectedFileName;
        private string _sheetName;
        private int _headerRow;
        private int _startRow;
        private int _endRow;

        public ImportRouteViewModel()
        {
            InitializeCommands();
            InitializeDefaults();
        }

        private void InitializeCommands()
        {
            CancelCommand = new RelayCommand(Cancel);
            NextCommand = new RelayCommand(Next, CanNext);
            DownloadTemplateCommand = new RelayCommand(DownloadTemplate);
        }

        private void InitializeDefaults()
        {
            HeaderRow = 0;
            StartRow = 0;
            EndRow = 0;
        }

        #region Properties
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
                OnPropertyChanged(nameof(FileSelectedVisibility));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                _selectedFileName = value;
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }

        public string SheetName
        {
            get => _sheetName;
            set
            {
                _sheetName = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }

        public int HeaderRow
        {
            get => _headerRow;
            set
            {
                _headerRow = value;
                OnPropertyChanged(nameof(HeaderRow));
            }
        }

        public int StartRow
        {
            get => _startRow;
            set
            {
                _startRow = value;
                OnPropertyChanged(nameof(StartRow));
            }
        }

        public int EndRow
        {
            get => _endRow;
            set
            {
                _endRow = value;
                OnPropertyChanged(nameof(EndRow));
            }
        }

        public Visibility FileSelectedVisibility
        {
            get => string.IsNullOrEmpty(SelectedFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Commands
        public ICommand CancelCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand DownloadTemplateCommand { get; private set; }

        public Action CloseAction { get; set; }
        public Action<List<RouteDetailModel>> FileImported { get; set; }
        #endregion

        #region Command Methods
        private void Cancel()
        {
            CloseAction?.Invoke();
        }

        private bool CanNext()
        {
            return !string.IsNullOrEmpty(SelectedFilePath);
        }

        private void Next()
        {
            try
            {
                // TODO: Implement Excel reading logic here
                // For now, just show a message
                MessageBox.Show(
                    $"File: {SelectedFileName}\nSheet: {SheetName}\nHeader: {HeaderRow}\nStart: {StartRow}\nEnd: {EndRow}",
                    "Import Settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // After successful import, call FileImported with the data
                // var routes = ReadExcelFile(SelectedFilePath, SheetName, HeaderRow, StartRow, EndRow);
                // FileImported?.Invoke(routes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi import file: {ex.Message}", "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadTemplate()
        {
            try
            {
                // TODO: Implement template download
                MessageBox.Show("Ch?c nãng t?i t?p m?u s? ðý?c tri?n khai sau", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Public Methods
        public void HandleFileDrop(string filePath)
        {
            if (IsValidExcelFile(filePath))
            {
                SelectedFilePath = filePath;
                SelectedFileName = Path.GetFileName(filePath);
            }
            else
            {
                MessageBox.Show("Vui l?ng ch?n file Excel (.xlsx, .xls, .xlsb, .xlsm)", "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void BrowseFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls;*.xlsb;*.xlsm",
                Title = "Ch?n file Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                SelectedFileName = Path.GetFileName(openFileDialog.FileName);
            }
        }

        private bool IsValidExcelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".xlsx" || extension == ".xls" || extension == ".xlsb" || extension == ".xlsm";
        }
        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
