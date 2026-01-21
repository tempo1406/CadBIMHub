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
                if (HeaderRow <= 0)
                {
                    MessageBox.Show("Vui lòng nhập dòng tiêu đề hợp lệ", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StartRow <= 0 || StartRow <= HeaderRow)
                {
                    MessageBox.Show("Vui lòng nhập dòng bắt đầu hợp lệ", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingRoutes = new List<RouteDetailModel>();
                try
                {
                    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    if (doc != null)
                    {
                        existingRoutes = DictionaryManager.LoadRoutesFromDrawing(doc.Database);
                    }
                }
                catch { }

                var validationData = ExcelHelper.ReadExcelFile(
                    SelectedFilePath, 
                    SheetName, 
                    HeaderRow, 
                    StartRow, 
                    EndRow,
                    existingRoutes);

                if (validationData == null || validationData.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu đã import", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // M? c?a s? validation
                var validationWindow = new Views.ImportValidationWindow();
                var validationVM = validationWindow.DataContext as ViewModels.ImportValidationViewModel;
                
                if (validationVM != null)
                {
                    validationVM.LoadValidationData(validationData);
                    validationVM.CloseAction = () => validationWindow.Close();
                    validationVM.OnImportSuccess = (routes) =>
                    {
                        FileImported?.Invoke(routes);
                    };
                }

                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(validationWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi import file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadTemplate()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = "Template_Import_Route.xlsx",
                    Title = "Lưu file template"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExcelHelper.ExportTemplate(saveFileDialog.FileName);
                    MessageBox.Show($"Đã tải xuống template thành công!", 
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
                DetectExcelStructureAuto();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn file Excel (.xlsx, .xls, .xlsb, .xlsm)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                
                DetectExcelStructureAuto();
            }
        }

        private void DetectExcelStructureAuto()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedFilePath))
                    return;

                var structureInfo = ExcelHelper.DetectExcelStructure(SelectedFilePath);
                
                if (structureInfo.IsValid)
                {
                    SheetName = structureInfo.SheetName;
                    HeaderRow = structureInfo.HeaderRow;
                    StartRow = structureInfo.StartRow;
                    EndRow = structureInfo.EndRow;
                }
                else
                {
                    MessageBox.Show(structureInfo.Message, "Cảnh báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phát hiện cấu trúc: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
