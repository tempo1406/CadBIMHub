using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using CadBIMHub.Helpers;
using CadBIMHub.Models;

namespace CadBIMHub.ViewModels
{
    public class CreateRouteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _selectedInstallationCondition;
        private string _selectedInstallationSpace;
        private string _selectedWorkPackage;
        private string _selectedPhase;
        private int _selectedCount;
        private bool _hasChanges;

        public CreateRouteViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            InitializeSampleData();
            
            RouteDetailModel.SelectionChanged += (s, e) => UpdateSelectedCount();
            
            RouteDetailList.CollectionChanged += (s, e) => HasChanges = true;
            BatchList.CollectionChanged += (s, e) => HasChanges = true;
        }

        private void InitializeCollections()
        {
            BatchList = new ObservableCollection<BatchInfoModel>();
            RouteDetailList = new ObservableCollection<RouteDetailModel>();
        }

        private void InitializeCommands()
        {
            SaveBatchCommand = new RelayCommand(SaveBatch);
            ImportCommand = new RelayCommand(Import);
            CreateNewCommand = new RelayCommand(CreateNew);
            DeleteCommand = new RelayCommand(Delete);
            DeleteRowCommand = new RelayCommand<RouteDetailModel>(DeleteRow);
            CopyRowCommand = new RelayCommand<RouteDetailModel>(CopyRow);
            CloseCommand = new RelayCommand(CloseDialog);
            AssignCommand = new RelayCommand(Assign);
            ToggleSelectAllCommand = new RelayCommand<bool?>(ToggleSelectAll);
        }

        #region Collections
        public ObservableCollection<BatchInfoModel> BatchList { get; set; }
        public ObservableCollection<RouteDetailModel> RouteDetailList { get; set; }
        #endregion

        #region Properties
        public string SelectedInstallationCondition
        {
            get => _selectedInstallationCondition;
            set
            {
                _selectedInstallationCondition = value;
                OnPropertyChanged(nameof(SelectedInstallationCondition));
            }
        }

        public string SelectedInstallationSpace
        {
            get => _selectedInstallationSpace;
            set
            {
                _selectedInstallationSpace = value;
                OnPropertyChanged(nameof(SelectedInstallationSpace));
            }
        }

        public string SelectedWorkPackage
        {
            get => _selectedWorkPackage;
            set
            {
                _selectedWorkPackage = value;
                OnPropertyChanged(nameof(SelectedWorkPackage));
            }
        }

        public string SelectedPhase
        {
            get => _selectedPhase;
            set
            {
                _selectedPhase = value;
                OnPropertyChanged(nameof(SelectedPhase));
            }
        }

        public int SelectedCount
        {
            get => _selectedCount;
            set
            {
                _selectedCount = value;
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(HasItemsSelected));
            }
        }

        public bool HasItemsSelected => SelectedCount > 0;

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }
        #endregion

        #region Commands
        public ICommand SaveBatchCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand CreateNewCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteRowCommand { get; private set; }
        public ICommand CopyRowCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand AssignCommand { get; private set; }
        public ICommand ToggleSelectAllCommand { get; private set; }

        public Action CloseAction { get; set; }
        #endregion

        #region Command Methods
        private void SaveBatch()
        {
            var batch = new BatchInfoModel
            {
                BatchCode = string.Format("B{0:D3}", BatchList.Count + 1),
                InstallationCondition = SelectedInstallationCondition,
                InstallationSpace = SelectedInstallationSpace,
                WorkPackage = SelectedWorkPackage,
                Phase = SelectedPhase
            };
            BatchList.Add(batch);
        }

        private void Import()
        {
            var importWindow = new Views.ImportRouteWindow();
            var importVM = importWindow.DataContext as ImportRouteViewModel;
            
            if (importVM != null)
            {
                importVM.CloseAction = () => importWindow.Close();
                importVM.FileImported = (importedRoutes) =>
                {
                    // Thêm các routes đã import vào danh sách
                    foreach (var route in importedRoutes)
                    {
                        route.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName != nameof(RouteDetailModel.IsSelected))
                            {
                                HasChanges = true;
                            }
                        };
                        RouteDetailList.Add(route);
                    }
                    
                    HasChanges = true;
                    importWindow.Close();
                };
            }
            
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(importWindow);
        }


        private void CreateNew()
        {
            var newDetail = new RouteDetailModel { };
            
            newDetail.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName != nameof(RouteDetailModel.IsSelected))
                {
                    HasChanges = true;
                }
            };
            
            RouteDetailList.Add(newDetail);
        }

        private void Delete()
        {
            var selectedItems = RouteDetailList.Where(x => x.IsSelected).ToList();
            foreach (var item in selectedItems)
            {
                RouteDetailList.Remove(item);
            }
            UpdateSelectedCount();
        }

        private void ToggleSelectAll(bool? isChecked)
        {
            bool selectAll = isChecked ?? false;
            foreach (var item in RouteDetailList)
            {
                item.IsSelected = selectAll;
            }
        }

        private void DeleteRow(RouteDetailModel detail)
        {
            if (detail != null)
            {
                RouteDetailList.Remove(detail);
                UpdateSelectedCount();
            }
        }

        private void CopyRow(RouteDetailModel detail)
        {
            if (detail != null)
            {
                var copy = new RouteDetailModel
                {
                    RouteName = detail.RouteName,
                    BatchNo = detail.BatchNo,
                    ItemGroup = detail.ItemGroup,
                    ItemDescription = detail.ItemDescription,
                    Size = detail.Size,
                    Symbol = detail.Symbol,
                    Quantity = detail.Quantity
                };
                RouteDetailList.Add(copy);
            }
        }

        private void CloseDialog()
        {
            CloseAction?.Invoke();
        }

        private void Assign()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    System.Windows.MessageBox.Show("Không tìm thấy document AutoCAD", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                Database db = doc.Database;

                DictionaryAction.SaveRoutesToDrawing(RouteDetailList.ToList(), db);

                DictionaryAction.SaveBatchesToDrawing(BatchList.ToList(), db);

                HasChanges = false;

                System.Windows.MessageBox.Show($"Đã lưu thành công {RouteDetailList.Count} routes và {BatchList.Count} batches vào Drawing!", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        private void InitializeSampleData()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    Database db = doc.Database;

                    var routesFromDict = DictionaryAction.LoadRoutesFromDrawing(db);
                    var batchesFromDict = DictionaryAction.LoadBatchesFromDrawing(db);

                    if (routesFromDict.Count > 0)
                    {
                        foreach (var route in routesFromDict)
                        {
                            route.PropertyChanged += (s, e) => 
                            {
                                if (e.PropertyName != nameof(RouteDetailModel.IsSelected))
                                {
                                    HasChanges = true;
                                }
                            };
                            
                            RouteDetailList.Add(route);
                        }

                        foreach (var batch in batchesFromDict)
                        {
                            BatchList.Add(batch);
                        }
                    }
                    else
                    {
                        try
                        {
                            LoadDataFromJson();
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    try
                    {
                        LoadDataFromJson();
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void LoadDataFromJson()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "requeset.json");
            
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Không tìm thấy file: {jsonPath}");
            }

            string jsonContent = File.ReadAllText(jsonPath);
            var response = SimpleJsonParser.ParseRouteData(jsonContent);

            if (response == null || response.Items == null || response.Items.Count == 0)
            {
                throw new Exception("Không có dữ liệu trong JSON");
            }

            RouteDetailList.Clear();

            foreach (var item in response.Items)
            {
                var routeDetail = new RouteDetailModel
                {
                    RouteName = item.Name,
                    BatchNo = item.BatchNo,
                    ItemGroup = item.ItemGroupName,
                    ItemDescription = item.ItemDescription,
                    Size = item.SizeName,
                    Symbol = item.Symbol,
                    Quantity = item.Quantity.ToString()
                };
                
                routeDetail.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName != nameof(RouteDetailModel.IsSelected))
                    {
                        HasChanges = true;
                    }
                };
                
                RouteDetailList.Add(routeDetail);
            }
        }

        public void UpdateSelectedCount()
        {
            SelectedCount = RouteDetailList.Count(x => x.IsSelected);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
