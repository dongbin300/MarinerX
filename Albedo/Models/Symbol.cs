namespace Albedo.Models
{
    public class Symbol
    {
        public string Market { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Diff { get; set; }

        public Symbol(string market, string name, decimal price, decimal diff)
        {
            Market = market;
            Name = name;
            Price = price;
            Diff = diff;
        }
    }
}
