namespace MarinerX.Bot.Bots
{
    public class ChartBot : Bot
    {
        public ChartBot() : this("", "")
        {

        }

        public ChartBot(string name) : this(name, "")
        {

        }

        public ChartBot(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
