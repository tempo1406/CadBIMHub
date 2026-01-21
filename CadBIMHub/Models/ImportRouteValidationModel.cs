using System.ComponentModel;

namespace CadBIMHub.Models
{
    public class ImportRouteValidationModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _validationMessage;
        private string _routeName;
        private string _batchNo;
        private string _itemGroup;
        private string _itemDescription;
        private string _size;
        private string _symbol;
        private string _quantity;
        private bool _isValid;

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

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
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

        public bool IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
