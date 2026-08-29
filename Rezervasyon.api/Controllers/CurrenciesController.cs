using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezervasyon.Api.Models;
using Rezervasyon.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrenciesController : ControllerBase
    {
        private readonly DataContext _context;
        public CurrenciesController(DataContext context)
        { _context = context; }

        [HttpGet]
        public IActionResult GetCurrencies() {
            var currencies = _context.Currencies.ToList();

            return Ok(currencies);

        }

        [HttpPost]
        public async Task<IActionResult> AddCurrency(Currency currency)
        {   if (currency == null)
                return BadRequest();

         _context.Add(currency);
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteCurrency(int id)
        {
            var currency= _context.Currencies.FirstOrDefault(x => x.Id==id);
            if(currency == null)
                return NotFound();
             _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpGet("{id}")]

        public IActionResult GetCurrency(int id)
        { 
            var currency= _context.Currencies.FirstOrDefault(x => x.Id==id);
            if(currency==null)
                return NotFound();
            
            return Ok(currency); }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency(int id,[FromBody] Currency currency)
        {
            if (id != currency.Id)
                return BadRequest();
            _context.Entry(currency).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Currencies.Any(x => x.Id == id))
                    return NotFound();
                throw;
            }
            return NoContent();
        }

        
    }
}
