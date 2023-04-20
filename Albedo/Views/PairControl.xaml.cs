using Albedo.Models;

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
        Pair symbol = default!;
        public Action<Pair> PairClick = default!;

        public PairControl()
        {
            InitializeComponent();
        }

        public void Init(Pair symbol)
        {
            this.symbol = symbol;
            viewModel.Init(symbol);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PairClick.Invoke(symbol);
        }
    }
}
