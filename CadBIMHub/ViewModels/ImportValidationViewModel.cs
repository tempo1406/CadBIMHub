using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CadBIMHub.Helpers;
using CadBIMHub.Models;

namespace CadBIMHub.ViewModels
{
    public class ImportValidationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _hasValidRoutes;
        private int _validCount;
        private int _invalidCount;

        public ImportValidationViewModel()
        {
            ValidationList = new ObservableCollection<ImportRouteValidationModel>();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            BackCommand = new RelayCommand(Back);
            ImportCommand = new RelayCommand(Import, CanImport);
        }

        #region Properties
        public ObservableCollection<ImportRouteValidationModel> ValidationList { get; set; }

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

        public Action CloseAction { get; set; }
        public Action<List<RouteDetailModel>> OnImportSuccess { get; set; }
        #endregion

        #region Commands
        public ICommand BackCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        #endregion

        #region Command Methods
        private void Back()
        {
            CloseAction?.Invoke();
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
            
            // Nếu có dòng không hợp lệ, hiển thị warning
            if (invalidCount > 0)
            {
                var message = $"Có {invalidCount}/{ValidationList.Count} dòng dữ liệu không hợp lệ. " +
                             $"Bạn có muốn bỏ qua các dòng dữ liệu này và chỉ thực hiện nhập dữ liệu ở các dòng còn lại không?";

                var result = MessageBox.Show(message, "Nhập dữ liệu từ tệp",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result != MessageBoxResult.OK)
                    return;
            }

            // Thực hiện import
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

            OnImportSuccess?.Invoke(routesToImport);

            MessageBox.Show($"Đã import thành công {routesToImport.Count} lộ!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);

            CloseAction?.Invoke();
        }
        #endregion

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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
