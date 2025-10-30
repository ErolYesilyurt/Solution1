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


      
        [HttpGet("calculate")]
        public async Task<ActionResult<PriceCalculationResult>> CalculatePrice([FromQuery] int hotelId, [FromQuery] DateTime giris, [FromQuery] DateTime cikis, [FromQuery] int YetiskinSayisi, [FromQuery] int CocukSayisi)
        {
            if (giris >= cikis)
            {
                return BadRequest("Giriş tarihi, çıkış tarihinden önce olmalıdır.");
            }

            if(CocukSayisi<0 || YetiskinSayisi<1 || YetiskinSayisi>3 || CocukSayisi>2)
            {  return BadRequest("Gecerli Yetiskin ve Cocuk sayisi girin."); }

            
            var priceInfo = await _context.Prices
                .FirstOrDefaultAsync(p => p.HotelId == hotelId &&
                                           giris >= p.GecerlilikBaslangic &&
                                           cikis <= p.GecerlilikBitis &&
                                           p.MaxGuests-(YetiskinSayisi+CocukSayisi)>=0);

            if (priceInfo == null)
            {
                return NotFound("Belirtilen tarihler için uygun bir fiyat bulunamadı.");
            }
            var a = priceInfo.Amount;
            var array = new decimal[][] {
    new decimal[] { a * 1.5m, a * 1.5m, 2 * a },               
    new decimal[] { 2 * a, 2 * a, 2 * a + (a / 2.0m) },       
    new decimal[] { 2 * a + a * 0.75m, 3 * a, 4 * a }         
};
            var geceSayisi = (cikis - giris).TotalDays;
            var toplamTutar = (decimal)geceSayisi * array[YetiskinSayisi-1][CocukSayisi];
          

            var result = new PriceCalculationResult
            {
                GecelikFiyat = priceInfo.Amount,
                GeceSayisi = (int)geceSayisi,
                ToplamTutar = toplamTutar,
                ParaBirimiKodu = (await _context.Currencies.FindAsync(priceInfo.CurrencyId))?.Code
            };

            return Ok(result);
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