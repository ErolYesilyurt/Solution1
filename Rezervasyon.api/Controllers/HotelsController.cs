using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezervasyon.Api.Models;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bu controller'a sadece giriş yapmış kullanıcılar erişebilir
    public class HotelsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHotels()
        {
            // TODO: Bu veriler daha sonra veritabanından gelecek.
            // Şimdilik test için sabit bir liste döndürüyoruz.
            var hotels = new List<Hotel>
            {
                new Hotel { Id = 1, Name = "Grand Park Lara", City = "Antalya", Stars = 5 },
                new Hotel { Id = 2, Name = "The Marmara Pera", City = "İstanbul", Stars = 4 },
                new Hotel { Id = 3, Name = "Swissôtel Uludağ", City = "Bursa", Stars = 5 }
            };

            return Ok(hotels);
        }
    }
}