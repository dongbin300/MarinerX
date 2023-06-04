using MarinerX.Bot.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace MarinerX.Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                //var memory = await WindowsMonitor.MemoryUsage().ConfigureAwait(false);
                //MemoryText.Text = $"{memory}MB";
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
