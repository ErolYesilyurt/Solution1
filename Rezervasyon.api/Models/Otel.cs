// Models/Hotel.cs
namespace Rezervasyon.Api.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Stars { get; set; }
    }
}