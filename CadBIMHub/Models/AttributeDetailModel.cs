using System.ComponentModel;

namespace CadBIMHub.Models
{
    public class AttributeDetailModel : INotifyPropertyChanged
    {
        private string _symbol;
        private string _quantity;

        public event PropertyChangedEventHandler PropertyChanged;

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
