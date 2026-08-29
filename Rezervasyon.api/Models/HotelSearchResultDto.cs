namespace Rezervasyon.Api.Models 
{
    public class HotelSearchResultDto
    {
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Stars { get; set; }

        
        public decimal GecelikFiyat { get; set; } 
        public decimal ToplamTutar { get; set; }
        public string ParaBirimiKodu { get; set; }
        public int GeceSayisi { get; set; }
    }
}