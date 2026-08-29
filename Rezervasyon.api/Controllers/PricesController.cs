using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezervasyon.Api.Data;
using Rezervasyon.Api.Models;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class PricesController : ControllerBase
    {
        private readonly DataContext _context;

        public PricesController(DataContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Price>> PostPrice([FromBody]Price price)
        {   if (price == null)
                return BadRequest();
            _context.Prices.Add(price);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPriceById", new { id = price.Id }, price); 
        }

        
        [HttpGet("forHotel/{hotelId}")]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<ActionResult<IEnumerable<Price>>> GetPricesForHotel(int hotelId)
        {
            return await _context.Prices
                .Where(p => p.HotelId == hotelId)
                .Include(p => p.Currency) 
                .ToListAsync();
        }



        [HttpPost("calculate")] // 1. GET'ten POST'a çevrildi
        public async Task<ActionResult<PriceCalculationResult>> CalculatePrice([FromBody] PriceCalculationRequest request) // 2. [FromBody] ile yeni modeli alır
        {
            // --- 1. Girdi Kontrolleri ---
            if (request.CheckInDate >= request.CheckOutDate)
            {
                return BadRequest("Giriş tarihi, çıkış tarihinden önce olmalıdır.");
            }
            if (request.GuestBirthDates == null || !request.GuestBirthDates.Any())
            {
                return BadRequest("En az bir misafir gereklidir.");
            }

            // --- 2. YAŞ HESAPLAMA (API Tarafında) ---
            // 12 yaş altı çocuk, 12 ve üzeri yetişkin varsayımı
            var yetiskinSayisi = request.GuestBirthDates.Count(dob => CalculateAge(dob, request.CheckInDate) >= 12);
            var cocukSayisi = request.GuestBirthDates.Count(dob => CalculateAge(dob, request.CheckInDate) < 12);
            var totalGuests = yetiskinSayisi + cocukSayisi;

            // --- 3. Fiyat ve Kapasite Kontrolü ---
            var priceInfo = await _context.Prices
                .FirstOrDefaultAsync(p => p.HotelId == request.HotelId &&
                                            request.CheckInDate >= p.GecerlilikBaslangic &&
                                            request.CheckOutDate <= p.GecerlilikBitis &&
                                            p.MaxGuests >= totalGuests); // MaxGuests (veya MaxKapasite) kontrolü

            if (priceInfo == null)
            {
                // Kapasite mi yetersiz, yoksa o tarihte fiyat mı yok? Kontrol et.
                bool priceExists = await _context.Prices.AnyAsync(p => p.HotelId == request.HotelId && request.CheckInDate >= p.GecerlilikBaslangic && request.CheckOutDate <= p.GecerlilikBitis);
                if (!priceExists)
                    return NotFound("Belirtilen tarihler için uygun bir fiyat tanımı bulunamadı.");
                else
                    return BadRequest($"Seçilen otel/tarihler için {totalGuests} kişilik kapasite bulunamadı.");
            }

            // --- 4. Fiyat Matrisi Hesaplaması ---
            var a = priceInfo.Amount;
            var array = new decimal[][] {
        new decimal[] { a * 1.5m, a * 1.5m, 2 * a },
        new decimal[] { 2 * a, 2 * a, 2 * a + (a / 2.0m) },
        new decimal[] { 2 * a + a * 0.75m, 3 * a, 4 * a }
    };

            int yetiskinIndex = yetiskinSayisi - 1;
            int cocukIndex = cocukSayisi;

            // Güvenlik kontrolü
            if (yetiskinIndex < 0 || yetiskinIndex >= array.Length || cocukIndex < 0 || cocukIndex >= array[yetiskinIndex].Length)
            {
                return BadRequest("Kişi sayısı (Yetişkin/Çocuk) fiyatlandırma kuralları dışındadır.");
            }

            decimal hesaplananGecelikFiyat = array[yetiskinIndex][cocukIndex] ; // VEYA SİZİN DOĞRU MATRİS MANTIĞINIZ

            var geceSayisi = (request.CheckOutDate - request.CheckInDate).TotalDays;
            var toplamTutar = (decimal)geceSayisi * hesaplananGecelikFiyat;

            var result = new PriceCalculationResult
            {
                GecelikFiyat = hesaplananGecelikFiyat, // Doğru hesaplanmış fiyatı döndür
                GeceSayisi = (int)geceSayisi,
                ToplamTutar = toplamTutar,
                ParaBirimiKodu = (await _context.Currencies.FindAsync(priceInfo.CurrencyId))?.Code
            };

            return Ok(result);
        }

        private int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > referenceDate.Date.AddYears(-age)) age--;
            return age;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<ActionResult<Price>> GetPriceById(int id)
        {
            var price = await _context.Prices.FindAsync(id);
            if (price == null) return NotFound();
            return price;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePrice(int id)
        { var price = _context.Prices.FirstOrDefault(x => x.Id == id);
            if (price == null)
                return BadRequest();
            _context.Prices.Remove(price);
           await _context.SaveChangesAsync();
            return Ok();

          }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePrice(int id,[FromBody] Price price)
        { if (price == null || id != price.Id)
                return BadRequest();

            _context.Entry(price).State = EntityState.Modified;
            try
            {
                _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if(!_context.Prices.Any(x => x.Id == id))
                    return NotFound();
                else
                    throw;
            }
            
            return NoContent();

        }
    }

    
    public class PriceCalculationResult
    {
        public decimal GecelikFiyat { get; set; }
        public int GeceSayisi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string ParaBirimiKodu { get; set; }
    }
}