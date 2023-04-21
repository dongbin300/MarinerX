using Albedo.Models;
using Albedo.ViewModels;

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Albedo.Views
{
    /// <summary>
    /// SymbolControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PairControl : UserControl
    {
        public PairControlViewModel viewModel;
        Pair pair = default!;
        public Action<Pair> PairClick = default!;

        public PairControl(Pair pair)
        {
            InitializeComponent();
            this.pair = pair;
            viewModel = new PairControlViewModel(pair);
            DataContext = viewModel;
            Name = $"{viewModel.Market}_{viewModel.MarketType}_{viewModel.Symbol}";
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PairClick.Invoke(pair);
        }
    }
}
