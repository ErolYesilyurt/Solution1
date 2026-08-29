using Rezervasyon.Client.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rezervasyon.Client.Models
{
    public class Reservation
    {
        public Reservation()
        {
            Guests = new HashSet<Guest>();
        }
        public int Id { get; set; }


        public DateTime GirisTarihi { get; set; }
        public DateTime CikisTarihi { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ToplamFiyat { get; set; }

        public string Aciklama { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public ICollection<Guest>? Guests { get; set; }
        public int HotelId { get; set; }
        public virtual Hotel Hotel { get; set; }

        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledOn { get; set; }

        public string CurrencyCode { get; set; }
    }
}