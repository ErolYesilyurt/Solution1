using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rezervasyon.Api.Models
{
    public class Reservation
    {
        public int Id { get; set; }

  
        public DateTime GirisTarihi { get; set; }
        public DateTime CikisTarihi { get; set; } 

   
        public int YetiskinSayisi { get; set; }
        public int CocukSayisi { get; set; }

      
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ToplamFiyat { get; set; }

        public string Aciklama { get; set; } 

        public int? UserId { get; set; }
        [ValidateNever]
        public virtual User? User { get; set; }

        public string? KisiAdSoyad { get; set; }
        public int HotelId { get; set; }
        [ValidateNever]
        public virtual Hotel Hotel { get; set; }
    }
}