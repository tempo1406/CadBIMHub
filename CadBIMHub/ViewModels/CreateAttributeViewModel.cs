using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CadBIMHub.Helpers;
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
            System.Windows.MessageBox.Show("Chức năng chọn đối tượng sẽ được triển khai sau", "Thông báo",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void Assign()
        {
            try
            {
                System.Windows.MessageBox.Show("Chức năng gán thuộc tính sẽ được triển khai sau", "Thông báo",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
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
                    _allRoutes = DictionaryManager.LoadRoutesFromDrawing(doc.Database);
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
