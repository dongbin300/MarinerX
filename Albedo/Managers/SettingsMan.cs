using Albedo.Enums;
using Albedo.Models;

using System;

namespace Albedo.Managers
{
    public class SettingsMan
    {
        public static int DefaultCandleCount { get; set; }
        public static int MaPeriod1 { get; set; }
        public static MaTypeModel MaType1 { get; set; } = default!;
        public static LineColorModel MaLineColor1 { get; set; } = default!;
        public static LineWeightModel MaLineWeight1 { get; set; } = default!;
        public static bool MaEnable1 { get; set; }

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

            DefaultCandleCount = Settings.Default.DefaultCandleCount;

            MaPeriod1 = Settings.Default.MaPeriod1;
            if (Enum.TryParse(typeof(MaType), Settings.Default.MaType1, out var maType1))
            {
                MaType1 = Common.MaTypes.Find(t => t.Type.Equals((MaType)maType1)) ?? default!;
            }
            if (Enum.TryParse(typeof(LineColor), Settings.Default.MaLineColor1, out var maLineColor1))
            {
                MaLineColor1 = Common.MaLineColors.Find(t => t.LineColor.Equals((LineColor)maLineColor1)) ?? default!;
            }
            if (Enum.TryParse(typeof(LineWeight), Settings.Default.MaLineWeight1, out var maLineWeight1))
            {
                MaLineWeight1 = Common.MaLineWeights.Find(t => t.LineWeight.Equals((LineWeight)maLineWeight1)) ?? default!;
            }
            MaEnable1 = Settings.Default.MaEnable1;
        }
    }
}
