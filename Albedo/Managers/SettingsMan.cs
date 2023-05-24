using Albedo.Enums;
using Albedo.Models;

using Newtonsoft.Json;

namespace Albedo.Managers
{
    public class SettingsMan
    {
        public static int DefaultCandleCount { get; set; }
        public static IndicatorsModel Indicators { get; set; } = default!;

        public SettingsMan()
        {

        }

        public static void Init()
        {
            Common.ChartInterval = Settings.Default.Interval switch
            {
                "1분" => CandleInterval.OneMinute,
                "3분" => CandleInterval.ThreeMinutes,
                "5분" => CandleInterval.FiveMinutes,
                "10분" => CandleInterval.TenMinutes,
                "15분" => CandleInterval.FifteenMinutes,
                "30분" => CandleInterval.ThirtyMinutes,
                "1시간" => CandleInterval.OneHour,
                "1일" => CandleInterval.OneDay,
                "1주" => CandleInterval.OneWeek,
                "1월" => CandleInterval.OneMonth,
                _ => CandleInterval.OneMinute,
            };

            Load();
        }

        public static void Load()
        {
            Indicators = JsonConvert.DeserializeObject<IndicatorsModel>(Settings.Default.IndicatorString) ?? default!;
            DefaultCandleCount = Settings.Default.DefaultCandleCount;
        }

        public static void Save()
        {
            Settings.Default.IndicatorString = JsonConvert.SerializeObject(Indicators);
            Settings.Default.Save();
        }
    }
}
