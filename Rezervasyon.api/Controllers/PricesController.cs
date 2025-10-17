using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        {  _context = context; }
        [HttpGet]
         public IActionResult GetPrices()
        {
            var prices= _context.Prices.ToList();
            return Ok(prices);

        }

        [HttpPost]
        public async Task<IActionResult> AddPrices([FromBody] Price price)
        {
            if (price == null)
                return BadRequest();
            _context.Prices.Add(price);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeletePrice(int id)
        {
            var price= _context.Prices.FirstOrDefault(x => x.Id == id);
            if (price == null)
                return NotFound();
            _context.Prices.Remove(price);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] Price price)
        {  if (price.Id != id)
                return BadRequest();

            _context.Entry(price).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Prices.Any(e => e.Id == id))
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
