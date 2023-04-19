using Albedo.Models;

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Albedo.Views
{
    /// <summary>
    /// SymbolControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SymbolControl : UserControl
    {
        Symbol symbol = default!;
        public Action<Symbol> SymbolClick = default!;

        public SymbolControl()
        {
            InitializeComponent();
        }

        public void Init(Symbol symbol)
        {
            this.symbol = symbol;
            viewModel.Init(symbol);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SymbolClick.Invoke(symbol);
        }
    }
}
