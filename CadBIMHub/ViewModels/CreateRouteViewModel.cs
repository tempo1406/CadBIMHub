using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CadBIMHub.Helpers;
using CadBIMHub.Models;

namespace CadBIMHub.ViewModels
{
    public class CreateRouteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _selectedDieuKien;
        private string _selectedKhongGian;
        private string _selectedGoiCongViec;
        private string _selectedGiaiDoan;
        private int _selectedCount;

        public CreateRouteViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            InitializeSampleData();
        }

        private void InitializeCollections()
        {
            DieuKienLapDatList = new ObservableCollection<string>
            {
                "1 gian giao",
                "2 gian giao",
                "3 gian giao",
                "Khong gian giao"
            };

            KhongGianLapDatList = new ObservableCollection<string>
            {
                "Lap am san",
                "Lap noi san",
                "Lap am tran",
                "Lap noi tran",
                "Lap tuong"
            };

            GoiCongViecList = new ObservableCollection<string>
            {
                "D_428_Bom",
                "D_428_10cm",
                "D_428_12cm",
                "D_315_8cm",
                "D_315_10cm"
            };

            GiaiDoanList = new ObservableCollection<string>
            {
                "CD1",
                "CD2",
                "CD3",
                "CD4"
            };

            NhomVatTuList = new ObservableCollection<string>
            {
                "Day & cap",
                "Ong",
                "Hop noi",
                "Phu kien"
            };

            VatTuList = new ObservableCollection<string>
            {
                "Cap tin",
                "Day dien",
                "Day cap",
                "Ong luon"
            };

            KichThuocList = new ObservableCollection<string>
            {
                "1x",
                "2x",
                "3x",
                "4x"
            };

            BatchList = new ObservableCollection<BatchInfoModel>();
            RouteDetailList = new ObservableCollection<RouteDetailModel>();

            SelectedDieuKien = DieuKienLapDatList.FirstOrDefault();
            SelectedKhongGian = KhongGianLapDatList.FirstOrDefault();
            SelectedGoiCongViec = GoiCongViecList.FirstOrDefault();
            SelectedGiaiDoan = GiaiDoanList.FirstOrDefault();
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
        }

        #region Collections
        public ObservableCollection<string> DieuKienLapDatList { get; set; }
        public ObservableCollection<string> KhongGianLapDatList { get; set; }
        public ObservableCollection<string> GoiCongViecList { get; set; }
        public ObservableCollection<string> GiaiDoanList { get; set; }
        public ObservableCollection<string> NhomVatTuList { get; set; }
        public ObservableCollection<string> VatTuList { get; set; }
        public ObservableCollection<string> KichThuocList { get; set; }
        public ObservableCollection<BatchInfoModel> BatchList { get; set; }
        public ObservableCollection<RouteDetailModel> RouteDetailList { get; set; }
        #endregion

        #region Properties
        public string SelectedDieuKien
        {
            get => _selectedDieuKien;
            set
            {
                _selectedDieuKien = value;
                OnPropertyChanged(nameof(SelectedDieuKien));
            }
        }

        public string SelectedKhongGian
        {
            get => _selectedKhongGian;
            set
            {
                _selectedKhongGian = value;
                OnPropertyChanged(nameof(SelectedKhongGian));
            }
        }

        public string SelectedGoiCongViec
        {
            get => _selectedGoiCongViec;
            set
            {
                _selectedGoiCongViec = value;
                OnPropertyChanged(nameof(SelectedGoiCongViec));
            }
        }

        public string SelectedGiaiDoan
        {
            get => _selectedGiaiDoan;
            set
            {
                _selectedGiaiDoan = value;
                OnPropertyChanged(nameof(SelectedGiaiDoan));
            }
        }

        public int SelectedCount
        {
            get => _selectedCount;
            set
            {
                _selectedCount = value;
                OnPropertyChanged(nameof(SelectedCount));
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

        public Action CloseAction { get; set; }
        #endregion

        #region Command Methods
        private void SaveBatch()
        {
            var batch = new BatchInfoModel
            {
                MaBatch = string.Format("B{0:D3}", BatchList.Count + 1),
                DieuKienLapDat = SelectedDieuKien,
                KhongGianLapDat = SelectedKhongGian,
                GoiCongViec = SelectedGoiCongViec,
                GiaiDoan = SelectedGiaiDoan
            };
            BatchList.Add(batch);
        }

        private void Import()
        {
            // TODO: Implement import functionality
        }

        private void CreateNew()
        {
            var newDetail = new RouteDetailModel
            {
                TenLo = (RouteDetailList.Count + 1).ToString(),
                Batch = "",
                NhomVatTu = "Day & cap",
                VatTu = "Day dien",
                KichThuoc = "1x",
                KyHieu = string.Format("D{0}", RouteDetailList.Count + 1),
                SoLuong = "1"
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
                    TenLo = detail.TenLo,
                    Batch = detail.Batch,
                    NhomVatTu = detail.NhomVatTu,
                    VatTu = detail.VatTu,
                    KichThuoc = detail.KichThuoc,
                    KyHieu = detail.KyHieu,
                    SoLuong = detail.SoLuong
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
            // TODO: Implement assign functionality
            CloseAction?.Invoke();
        }
        #endregion

        private void InitializeSampleData()
        {
            RouteDetailList.Add(new RouteDetailModel { TenLo = "1", Batch = "", NhomVatTu = "Day & cap", VatTu = "Cap tin", KichThuoc = "1x", KyHieu = "D2", SoLuong = "1" });
            RouteDetailList.Add(new RouteDetailModel { TenLo = "2", Batch = "", NhomVatTu = "Day & cap", VatTu = "Day dien", KichThuoc = "1x", KyHieu = "D1", SoLuong = "2" });
            RouteDetailList.Add(new RouteDetailModel { TenLo = "3", Batch = "", NhomVatTu = "Day & cap", VatTu = "Day dien", KichThuoc = "2x", KyHieu = "A", SoLuong = "3" });
            RouteDetailList.Add(new RouteDetailModel { TenLo = "4", Batch = "", NhomVatTu = "Day & cap", VatTu = "Day dien", KichThuoc = "2x", KyHieu = "C6", SoLuong = "4" });
            RouteDetailList.Add(new RouteDetailModel { TenLo = "5", Batch = "", NhomVatTu = "Day & cap", VatTu = "Day dien", KichThuoc = "1x", KyHieu = "D2", SoLuong = "1" });
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
