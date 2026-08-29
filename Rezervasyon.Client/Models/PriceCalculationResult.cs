namespace Rezervasyon.Client.Models
{
   
    public class PriceCalculationResult
    {
        public decimal GecelikFiyat { get; set; }
        public int GeceSayisi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string ParaBirimiKodu { get; set; }
    }
}