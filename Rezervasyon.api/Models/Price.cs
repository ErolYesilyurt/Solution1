using System.ComponentModel.DataAnnotations.Schema;

namespace Rezervasyon.Api.Models
{
    public class Price
    { public int Id { get; set; }

    

        [Column(TypeName= "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public int HotelId { get; set; }

        public Hotel Hotel { get; set; } 

        public int CurrencyId { get; set; }

        public Currency Currency { get; set; }



        
        

    }
}
