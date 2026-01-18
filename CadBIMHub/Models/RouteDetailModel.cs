using System;
using System.ComponentModel;

namespace CadBIMHub.Models
{
    public class RouteDetailModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _routeName;
        private string _batchNo;
        private string _itemGroup;
        private string _itemDescription;
        private string _size;
        private string _symbol;
        private string _quantity;

        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler SelectionChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string RouteName
        {
            get => _routeName;
            set
            {
                _routeName = value;
                OnPropertyChanged(nameof(RouteName));
            }
        }

        public string BatchNo
        {
            get => _batchNo;
            set
            {
                _batchNo = value;
                OnPropertyChanged(nameof(BatchNo));
            }
        }

        public string ItemGroup
        {
            get => _itemGroup;
            set
            {
                _itemGroup = value;
                OnPropertyChanged(nameof(ItemGroup));
            }
        }

        public string ItemDescription
        {
            get => _itemDescription;
            set
            {
                _itemDescription = value;
                OnPropertyChanged(nameof(ItemDescription));
            }
        }

        public string Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        public string Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

