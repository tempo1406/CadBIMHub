using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CadBIMHub.Helpers;
using CadBIMHub.Models;
using CadBIMHub.MVVM;
using CadBIMHub.Services;
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
        private int _currentStep;
        private bool _hasValidRoutes;
        private int _validCount;
        private int _invalidCount;

        public ImportRouteViewModel()
        {
            InitializeCommands();
            InitializeDefaults();
            ValidationList = new ObservableCollection<ImportRouteValidationModel>();
        }

        private void InitializeCommands()
        {
            CancelCommand = new RelayCommand(Cancel);
            NextCommand = new RelayCommand(Next, CanNext);
            DownloadTemplateCommand = new RelayCommand(DownloadTemplate);
            BackCommand = new RelayCommand(Back);
            ImportCommand = new RelayCommand(Import, CanImport);
        }

        private void InitializeDefaults()
        {
            HeaderRow = 0;
            StartRow = 0;
            EndRow = 0;
            CurrentStep = 1;
        }

        #region Properties
        public ObservableCollection<ImportRouteValidationModel> ValidationList { get; set; }

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

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
            }
        }

        public bool IsStep1 => CurrentStep == 1;
        public bool IsStep2 => CurrentStep == 2;

        public Visibility FileSelectedVisibility
        {
            get => string.IsNullOrEmpty(SelectedFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool HasValidRoutes
        {
            get => _hasValidRoutes;
            set
            {
                _hasValidRoutes = value;
                OnPropertyChanged(nameof(HasValidRoutes));
            }
        }

        public int ValidCount
        {
            get => _validCount;
            set
            {
                _validCount = value;
                OnPropertyChanged(nameof(ValidCount));
            }
        }

        public int InvalidCount
        {
            get => _invalidCount;
            set
            {
                _invalidCount = value;
                OnPropertyChanged(nameof(InvalidCount));
            }
        }
        #endregion

        #region Commands
        public ICommand CancelCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand DownloadTemplateCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }

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
                        existingRoutes = DictionaryService.LoadRoutesFromDrawing(doc.Database);
                    }
                }
                catch { }

                var validationData = ExcelService.ReadExcelFile(
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

                LoadValidationData(validationData);
                CurrentStep = 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi import file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back()
        {
            CurrentStep = 1;
        }

        private bool CanImport()
        {
            return ValidationList.Any(v => v.IsValid);
        }

        private void Import()
        {
            var validRoutes = ValidationList.Where(v => v.IsValid).ToList();

            if (validRoutes.Count == 0)
            {
                MessageBox.Show("Không có lộ hợp lệ để import", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var invalidCount = ValidationList.Count - validRoutes.Count;
            
            if (invalidCount > 0)
            {
                var message = $"Có {invalidCount}/{ValidationList.Count} dòng dữ liệu không hợp lệ. " +
                             $"Bạn có muốn bỏ qua các dòng dữ liệu này và chỉ thực hiện nhập dữ liệu ở các dòng còn lại không?";

                var result = MessageBox.Show(message, "Nhập dữ liệu từ tệp",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result != MessageBoxResult.OK)
                    return;
            }

            var routesToImport = validRoutes.Select(v => new RouteDetailModel
            {
                RouteName = v.RouteName,
                BatchNo = v.BatchNo,
                ItemGroup = v.ItemGroup,
                ItemDescription = v.ItemDescription,
                Size = v.Size,
                Symbol = v.Symbol,
                Quantity = v.Quantity
            }).ToList();

            FileImported?.Invoke(routesToImport);

            MessageBox.Show($"Đã import thành công {routesToImport.Count} lộ!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);

            CloseAction?.Invoke();
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
                    ExcelService.ExportTemplate(saveFileDialog.FileName);
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
                Title = "Chọn file Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                SelectedFileName = Path.GetFileName(openFileDialog.FileName);
                
                DetectExcelStructureAuto();
            }
        }

        public void LoadValidationData(List<ImportRouteValidationModel> data)
        {
            ValidationList.Clear();
            foreach (var item in data)
            {
                ValidationList.Add(item);
            }

            ValidCount = ValidationList.Count(v => v.IsValid);
            InvalidCount = ValidationList.Count - ValidCount;
            HasValidRoutes = ValidCount > 0;
        }

        private void DetectExcelStructureAuto()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedFilePath))
                    return;

                var structureInfo = ExcelService.DetectExcelStructure(SelectedFilePath);
                
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
