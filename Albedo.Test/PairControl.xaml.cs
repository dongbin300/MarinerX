using System.Windows.Controls;

namespace Albedo.Test
{
    /// <summary>
    /// PairControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PairControl : UserControl
    {
        public Pair Pair { get; set; }

        public PairControl()
        {
            InitializeComponent();
        }

        public void Init(string symbol, string price)
        {
            Pair = new Pair(symbol, price);
        }
    }
}
