namespace MarketDataApp.Models
{
    public class MarketData
    {
        public string Indicador { get; set; }
        public string Data { get; set; }
        public string DataReferencia { get; set; }
        public string? Reuniao { get; set; }
        public double Media { get; set; }
        public double Mediana { get; set; }
        public double DesvioPadrao { get; set; }
        public double Minimo { get; set; }
        public double Maximo { get; set; }
        public double baseCalculo { get; set; }
    }
}
