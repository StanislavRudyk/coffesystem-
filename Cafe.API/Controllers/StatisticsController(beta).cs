using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cafe.API.Data;

namespace Cafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public StatisticsController(AppDbContext context) => _context = context;

        //Общая выручка за всё время
        [HttpGet("total-revenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            return await _context.OrderItems.SumAsync(item => item.PriceAtSale * item.Quantity);
        }

        // Статистика продаж по категориям 
        [HttpGet("sales-by-category")]
        public async Task<ActionResult> GetSalesByCategory()
        {
            var stats = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalSales = g.Sum(oi => oi.PriceAtSale * oi.Quantity),
                    Count = g.Sum(oi => oi.Quantity)
                })
                .ToListAsync();

            return Ok(stats);
        }

        //Топ-5 самых продаваемых товаров
        [HttpGet("top-products")]
        public async Task<ActionResult> GetTopProducts()
        {
            var top = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            return Ok(top);
        }
    }
}