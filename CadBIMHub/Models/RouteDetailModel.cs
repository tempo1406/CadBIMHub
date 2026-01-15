using System.ComponentModel;

namespace CadBIMHub.Models
{
    public class RouteDetailModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _tenLo;
        private string _batch;
        private string _nhomVatTu;
        private string _vatTu;
        private string _kichThuoc;
        private string _kyHieu;
        private string _soLuong;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string TenLo
        {
            get => _tenLo;
            set
            {
                _tenLo = value;
                OnPropertyChanged(nameof(TenLo));
            }
        }

        public string Batch
        {
            get => _batch;
            set
            {
                _batch = value;
                OnPropertyChanged(nameof(Batch));
            }
        }

        public string NhomVatTu
        {
            get => _nhomVatTu;
            set
            {
                _nhomVatTu = value;
                OnPropertyChanged(nameof(NhomVatTu));
            }
        }

        public string VatTu
        {
            get => _vatTu;
            set
            {
                _vatTu = value;
                OnPropertyChanged(nameof(VatTu));
            }
        }

        public string KichThuoc
        {
            get => _kichThuoc;
            set
            {
                _kichThuoc = value;
                OnPropertyChanged(nameof(KichThuoc));
            }
        }

        public string KyHieu
        {
            get => _kyHieu;
            set
            {
                _kyHieu = value;
                OnPropertyChanged(nameof(KyHieu));
            }
        }

        public string SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged(nameof(SoLuong));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
