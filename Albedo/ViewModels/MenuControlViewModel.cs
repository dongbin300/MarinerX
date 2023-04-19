using Albedo.Models;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Albedo.ViewModels
{
    public class MenuControlViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notify Property Changed

        private ObservableCollection<Symbol> symbols = new ();
        public ObservableCollection<Symbol> Symbols
        {
            get => symbols;
            set
            {
                symbols = value;
                OnPropertyChanged(nameof(Symbols));
            }
        }

        public MenuControlViewModel()
        {

        }

        public void UpdateSymbolInfo(Symbol symbol)
        {
            var _symbol = Symbols.Where(s => s.Market.Equals(symbol.Market) && s.Name.Equals(symbol.Name));
            if (_symbol == null || !_symbol.Any())
            {
                Symbols.Add(symbol);
                return;
            }

            _symbol.ElementAt(0).Price = symbol.Price;
            _symbol.ElementAt(0).Diff = symbol.Diff;
        }
    }
}
