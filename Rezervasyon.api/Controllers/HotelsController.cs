using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezervasyon.Api.Models;
using Rezervasyon.Api.Data;
using Microsoft.EntityFrameworkCore;
using Rezervasyon.Api.Models;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class HotelsController : ControllerBase
    {   private readonly DataContext _context;

        public HotelsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetHotels()
        {   
           

            var hotels =_context.Oteller.ToList();

            return Ok(hotels);
        }

        [HttpGet("search-with-price")]
        public async Task<ActionResult<IEnumerable<HotelSearchResultDto>>> SearchAvailableHotelsWithPrice(
    [FromQuery] string destination,
    [FromQuery] DateTime checkIn,
    [FromQuery] DateTime checkOut,
    [FromQuery] int adults = 1,
    [FromQuery] int children = 0)
        {
            if (string.IsNullOrWhiteSpace(destination))
                return BadRequest("Hedef boş olamaz.");

            if(checkIn >= checkOut)
                return BadRequest("Giriş tarihi, çıkış tarihinden önce olmalıdır.");

            if(adults < 1 || adults > 3 || children < 0 || children > 2)
                return BadRequest("Geçerli yetişkin ve çocuk sayısı girin.");

            var numberOfNights = (checkOut - checkIn).TotalDays;
            int adultIndex = adults - 1;
            int childIndex = children;

            var query = _context.Oteller
        .Where(h => h.Name.Contains(destination) || h.City.Contains(destination))
      
        .Where(h => _context.Prices.Any(p => p.HotelId == h.Id &&
                                               checkIn >= p.GecerlilikBaslangic &&
                                               checkOut <= p.GecerlilikBitis))

        .Where(h => _context.Reservations
                          .Count(r => r.HotelId == h.Id &&
                                      checkIn < r.CikisTarihi &&
                                      checkOut > r.GirisTarihi) < 5)
        .Where(h => !_context.StopSales.Any(ss => ss.HotelId == h.Id &&
                                                   checkIn < ss.BitisTarihi &&
                                                   checkOut > ss.BaslangicTarihi));

            var potentialHotels = await query
                .Select(h => new 
                {
                    Hotel = h,
                    
                    PriceInfo = _context.Prices
                                    .Include(p => p.Currency) 
                                    .FirstOrDefault(p => p.HotelId == h.Id &&
                                                           checkIn >= p.GecerlilikBaslangic &&
                                                           checkOut <= p.GecerlilikBitis)
                })
                .ToListAsync();

           
            var results = new List<HotelSearchResultDto>();

            
            var priceMatrix = new decimal[][] {
        new decimal[] { 1.5m, 1.5m, 2.0m },
        new decimal[] { 2.0m, 2.0m, 2.0m + (1m / 2.0m) },
        new decimal[] { 2.0m + 0.75m, 3.0m, 4.0m }
    };

            foreach (var item in potentialHotels)
            {
                if (item.PriceInfo == null) continue; 

                var baseAmount = item.PriceInfo.Amount;

               
                if (adultIndex >= priceMatrix.Length || childIndex >= priceMatrix[adultIndex].Length)
                {
                    Console.WriteLine($"Uyarı: Otel ID {item.Hotel.Id} için fiyat matrisi dışında kalan kişi sayısı ({adults},{children}). Atlanıyor.");
                    continue; 
                }

               
                decimal calculatedNightlyRate = priceMatrix[adultIndex][childIndex]*baseAmount; 
                                                                                    

                var totalPrice = calculatedNightlyRate * (decimal)numberOfNights;

                results.Add(new HotelSearchResultDto
                {
                    Id = item.Hotel.Id,
                    Name = item.Hotel.Name,
                    City = item.Hotel.City,
                    Stars = item.Hotel.Stars,
                    GecelikFiyat = calculatedNightlyRate,
                    ToplamTutar = totalPrice,
                    ParaBirimiKodu = item.PriceInfo.Currency?.Code ?? "???",
                    GeceSayisi = (int)numberOfNights
                });
            }

            return Ok(results);

        }
          

            [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AddHotel([FromBody] Hotel hotel)
        {
            if (hotel ==null)
            { return BadRequest(); }

            _context.Oteller.Add(hotel);
            await _context.SaveChangesAsync();
            return Ok(hotel);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles="Admin")]

        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Oteller.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            _context.Oteller.Remove(hotel);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotel(int id)
        {
            var hotel = await _context.Oteller.FirstOrDefaultAsync(a => a.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }

        [HttpPut("{id}")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] Hotel updatedHotel)
        {
           if(id != updatedHotel.Id)
            {
                return BadRequest();
            }
            
           _context.Entry(updatedHotel).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {  if(!_context.Oteller.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
       
            return NoContent();

        }

    }
}