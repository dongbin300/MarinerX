using System.Windows.Controls;

namespace Albedo.Views.Settings
{
    /// <summary>
    /// SettingsMarketControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsMarketControl : UserControl
    {
        public SettingsMarketControl()
        {
            InitializeComponent();

            BinanceApiKeyText.Text = Albedo.Settings.Default.BinanceApiKey;
            BinanceApiSecretKeyText.Text = Albedo.Settings.Default.BinanceSecretKey;
            UpbitApiKeyText.Text = Albedo.Settings.Default.UpbitApiKey;
            UpbitApiSecretKeyText.Text = Albedo.Settings.Default.UpbitSecretKey;
            BithumbApiKeyText.Text = Albedo.Settings.Default.BithumbApiKey;
            BithumbApiSecretKeyText.Text = Albedo.Settings.Default.BithumbSecretKey;
        }
    }
}
