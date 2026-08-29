using System.ComponentModel.DataAnnotations;

namespace Rezervasyon.Client.Models
{
    public class StopSale
    {
        public int Id { get; set; }

        [Required]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        public DateTime BitisTarihi { get; set; }

        public string? Aciklama { get; set; }


        [Required]
        public int HotelId { get; set; }
        
        public virtual Hotel Hotel { get; set; }
    }
}
