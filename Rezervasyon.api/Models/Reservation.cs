using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rezervasyon.Api.Models
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
        [ValidateNever]
        public virtual User? User { get; set; }
        public ICollection<Guest>? Guests { get; set; }
        public int HotelId { get; set; }
        [ValidateNever]
        public virtual Hotel Hotel { get; set; }

        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledOn { get; set; }

        public int? PriceId { get; set; }
        [ValidateNever]
        public virtual Price? Price { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal IptalKesintisi { get; set; } = 0;

        [Required]
        public string CurrencyCode { get; set; } = "TRY";
    }
}