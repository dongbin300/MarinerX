using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Albedo.Test
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notify Property Changed

        private ObservableCollection<PairControl> pairs = new();
        public ObservableCollection<PairControl> Pairs
        {
            get => pairs;
            set
            {
                pairs = value;
                OnPropertyChanged(nameof(Pairs));
            }
        }

        public MainWindowViewModel()
        {

        }

        public void UpdatePair(string symbol, string price)
        {
            var pair = Pairs.Where(p => p.Pair.Symbol.Equals(symbol));

            if (pair == null || !pair.Any())
            {
                var pairControl = new PairControl();
                pairControl.Init(symbol, price);
                Pairs.Add(pairControl);
                return;
            }

            pair.ElementAt(0).Pair.Price = price;
        }
    }
}
