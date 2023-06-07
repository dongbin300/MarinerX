using System;

namespace MarinerX.Bot.Models
{
    public class BotHistory
    {
        public DateTime DateTime { get; set; }
        public string Time => DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string Text { get; set; }

        public BotHistory(DateTime dateTime, string text)
        {
            DateTime = dateTime;
            Text = text;
        }
    }
}
