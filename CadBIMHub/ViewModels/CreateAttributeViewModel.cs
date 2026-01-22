using System;
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
            // TODO: Implement object selection logic
            System.Windows.MessageBox.Show("Chức năng chọn đối tượng sẽ được triển khai sau", "Thông báo",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void Assign()
        {
            try
            {
                // TODO: Implement assign logic
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

        private void LoadRouteNames()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    var routes = DictionaryManager.LoadRoutesFromDrawing(doc.Database);
                    RouteNameList.Clear();
                    
                    foreach (var route in routes)
                    {
                        if (!string.IsNullOrEmpty(route.RouteName) && !RouteNameList.Contains(route.RouteName))
                        {
                            RouteNameList.Add(route.RouteName);
                        }
                    }

                    if (RouteNameList.Count > 0)
                    {
                        SelectedRouteName = RouteNameList[0];
                    }
                }
            }
            catch (Exception)
            {
                RouteNameList.Add("s1");
                RouteNameList.Add("s2");
                RouteNameList.Add("s3");
                SelectedRouteName = "s2";
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
