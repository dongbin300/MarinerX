using System.Windows.Controls;

namespace Albedo.Views.Settings
{
    /// <summary>
    /// SettingsMarketControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsMarketControl : UserControl
    {
        bool isInit = false;

        public SettingsMarketControl()
        {
            InitializeComponent();

            isInit = true;
            BinanceApiKeyText.Text = Albedo.Settings.Default.BinanceApiKey;
            BinanceApiSecretKeyText.Text = Albedo.Settings.Default.BinanceSecretKey;
            UpbitApiKeyText.Text = Albedo.Settings.Default.UpbitApiKey;
            UpbitApiSecretKeyText.Text = Albedo.Settings.Default.UpbitSecretKey;
            BithumbApiKeyText.Text = Albedo.Settings.Default.BithumbApiKey;
            BithumbApiSecretKeyText.Text = Albedo.Settings.Default.BithumbSecretKey;
            isInit = false;
        }

        private void ApiKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            Albedo.Settings.Default.BinanceApiKey = BinanceApiKeyText.Text;
            Albedo.Settings.Default.BinanceSecretKey = BinanceApiSecretKeyText.Text;
            Albedo.Settings.Default.UpbitApiKey = UpbitApiKeyText.Text;
            Albedo.Settings.Default.UpbitSecretKey = UpbitApiSecretKeyText.Text;
            Albedo.Settings.Default.BithumbApiKey = BithumbApiKeyText.Text;
            Albedo.Settings.Default.BithumbSecretKey = BithumbApiSecretKeyText.Text;
            Albedo.Settings.Default.Save();
        }
    }
}
