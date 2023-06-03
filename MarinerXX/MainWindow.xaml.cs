using MarinerXX.Apis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarinerXX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var symbols = LocalStorageApi.SymbolNames;
            foreach (var symbol in symbols)
            {
                var quotes = LocalStorageApi.GetQuotes(symbol, DateTime.Parse("2022-01-01"));

                if(quotes != null)
                {

                }
            }
        }
    }
}
