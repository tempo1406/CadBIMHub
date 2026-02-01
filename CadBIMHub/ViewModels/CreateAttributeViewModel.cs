using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CadBIMHub.Services;
using CadBIMHub.MVVM;
using CadBIMHub.Models;

namespace CadBIMHub.ViewModels
{
    public class CreateAttributeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _selectedRouteName;
        private bool _isSelectByObject;
        private bool _isSelectByLayer;
        private List<RouteDetailModel> _allRoutes;
        private int _selectedObjectCount;

        public CreateAttributeViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            InitializeDefaults();
        }

        private void InitializeCollections()
        {
            RouteNameList = new ObservableCollection<string>();
            AttributeDetailList = new ObservableCollection<AttributeDetailModel>();
            
            LoadRoutesFromDrawing();
            LoadRouteNames();
        }

        private void InitializeCommands()
        {
            CreateNewCommand = new RelayCommand(CreateNew);
            DeleteRowCommand = new RelayCommand<AttributeDetailModel>(DeleteRow);
            SelectObjectCommand = new RelayCommand(SelectObject);
            AssignCommand = new RelayCommand(Assign);
            CloseCommand = new RelayCommand(CloseDialog);
        }

        private void InitializeDefaults()
        {
            IsSelectByObject = true;
            IsSelectByLayer = false;
        }

        #region Collections
        public ObservableCollection<string> RouteNameList { get; set; }
        public ObservableCollection<AttributeDetailModel> AttributeDetailList { get; set; }
        #endregion

        #region Properties
        public string SelectedRouteName
        {
            get => _selectedRouteName;
            set
            {
                _selectedRouteName = value;
                OnPropertyChanged(nameof(SelectedRouteName));
                LoadRoutesByName(value);
            }
        }

        public bool IsSelectByObject
        {
            get => _isSelectByObject;
            set
            {
                _isSelectByObject = value;
                OnPropertyChanged(nameof(IsSelectByObject));
            }
        }

        public bool IsSelectByLayer
        {
            get => _isSelectByLayer;
            set
            {
                _isSelectByLayer = value;
                OnPropertyChanged(nameof(IsSelectByLayer));
            }
        }

        public int SelectedObjectCount
        {
            get => _selectedObjectCount;
            set
            {
                _selectedObjectCount = value;
                OnPropertyChanged(nameof(SelectedObjectCount));
                OnPropertyChanged(nameof(SelectedObjectCountText));
            }
        }

        public string SelectedObjectCountText
        {
            get => SelectedObjectCount > 0 ? $"Đã chọn {SelectedObjectCount} đối tượng" : "Chưa chọn đối tượng";
        }
        #endregion

        #region Commands
        public ICommand CreateNewCommand { get; private set; }
        public ICommand DeleteRowCommand { get; private set; }
        public ICommand SelectObjectCommand { get; private set; }
        public ICommand AssignCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public Action CloseAction { get; set; }
        #endregion

        #region Command Methods
        private void CreateNew()
        {
            var newAttribute = new AttributeDetailModel();
            AttributeDetailList.Add(newAttribute);
        }

        private void DeleteRow(AttributeDetailModel attribute)
        {
            if (attribute != null)
            {
                AttributeDetailList.Remove(attribute);
            }
        }

        private void SelectObject()
        {
            try
            {
                int count;
                bool success;
                string layerName = string.Empty;

                if (IsSelectByObject)
                {
                    success = AttributeService.SelectPolylineOrMLine(out count);
                    SelectedObjectCount = success ? count : 0;
                }
                else if (IsSelectByLayer)
                {
                    success = AttributeService.SelectPolylineOrMLineByLayer(out count, out layerName);
                    SelectedObjectCount = success ? count : 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi chọn đối tượng: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Assign()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedRouteName))
                {
                    System.Windows.MessageBox.Show("Vui lòng chọn tên lộ!", "Cảnh báo",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (AttributeDetailList == null || AttributeDetailList.Count == 0)
                {
                    System.Windows.MessageBox.Show("Danh sách thuộc tính trống!", "Cảnh báo",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                int selectedCount = AttributeService.GetSelectedCount();
                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("Vui lòng chọn đối tượng trước khi gán!", "Cảnh báo",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var routesForAttribute = _allRoutes?
                    .Where(r => r.RouteName == SelectedRouteName)
                    .ToList();

                if (routesForAttribute == null || routesForAttribute.Count == 0)
                {
                    System.Windows.MessageBox.Show("Không tìm thấy thông tin route!", "Cảnh báo",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                CloseAction?.Invoke();

                AttributeService.AssignAttributes(
                    SelectedRouteName,
                    AttributeDetailList.ToList(),
                    routesForAttribute,
                    (current, total) =>
                    {
                    });

                System.Windows.MessageBox.Show("Gán thuộc tính thành công!", "Thành công",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                AttributeService.ClearSelection();
                SelectedObjectCount = 0;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi gán thuộc tính: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CloseDialog()
        {
            CloseAction?.Invoke();
        }
        #endregion

        private void LoadRoutesFromDrawing()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    _allRoutes = DictionaryService.LoadRoutesFromDrawing(doc.Database);
                }
                else
                {
                    _allRoutes = new List<RouteDetailModel>();
                }
            }
            catch (Exception)
            {
                _allRoutes = new List<RouteDetailModel>();
            }
        }

        private void LoadRouteNames()
        {
            try
            {
                RouteNameList.Clear();
                
                if (_allRoutes != null && _allRoutes.Count > 0)
                {
                    var uniqueRouteNames = _allRoutes
                        .Where(r => !string.IsNullOrEmpty(r.RouteName))
                        .Select(r => r.RouteName)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();

                    foreach (var routeName in uniqueRouteNames)
                    {
                        RouteNameList.Add(routeName);
                    }

                    if (RouteNameList.Count > 0)
                    {
                        SelectedRouteName = RouteNameList[0];
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void LoadRoutesByName(string routeName)
        {
            AttributeDetailList.Clear();

            if (string.IsNullOrEmpty(routeName) || _allRoutes == null)
                return;

            var routesForSelectedName = _allRoutes
                .Where(r => r.RouteName == routeName)
                .ToList();

            foreach (var route in routesForSelectedName)
            {
                var attribute = new AttributeDetailModel
                {
                    Symbol = route.Symbol,
                    Quantity = route.Quantity
                };
                AttributeDetailList.Add(attribute);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
