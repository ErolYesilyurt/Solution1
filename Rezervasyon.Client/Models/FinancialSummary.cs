namespace Rezervasyon.Client.Models
{
    public class FinancialSummary
    {

        public decimal ToplamKazanc { get; set; }
        public int AktifRezervasyonSayisi { get; set; }
        public decimal IptalEdilenTutar { get; set; }
        public int IptalEdilenRezervasyonSayisi { get; set; }
        public decimal PotansiyelKazanc { get; set; }
        public int ToplamRezervasyonKaydi { get; set; }
    }
}
