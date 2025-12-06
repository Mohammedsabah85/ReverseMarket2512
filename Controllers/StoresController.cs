using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Controllers
{
    public class StoresController : BaseController
    {
        public StoresController(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IActionResult> Index(string search, int? categoryId, int page = 1)
        {
            var pageSize = 12;

            var query = _context.Users
           .Where(u => u.UserType == UserType.Seller &&
                      !string.IsNullOrEmpty(u.StoreName) &&
                      u.IsActive &&
                      u.IsStoreApproved)
           .Include(u => u.StoreCategories)
           .ThenInclude(sc => sc.Category)
           .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.StoreName.Contains(search) ||
                                        u.StoreDescription.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(u => u.StoreCategories.Any(sc => sc.CategoryId == categoryId.Value));
            }

            var totalStores = await query.CountAsync();
            var stores = await query
                .OrderBy(u => u.StoreName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new StoresViewModel
            {
                Stores = stores,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalStores / pageSize),
                Search = search,
                SelectedCategoryId = categoryId,

                Advertisements = await _context.Advertisements
                    .Where(a => a.IsActive &&
                           a.StartDate <= DateTime.Now &&
                           (a.EndDate == null || a.EndDate >= DateTime.Now))
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync(),
            };

            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            var store = await _context.Users
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.Category)
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.SubCategory1)
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.SubCategory2)
                .FirstOrDefaultAsync(u => u.Id == id &&
                                         u.UserType == UserType.Seller &&
                                         u.IsActive);

            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }
    }
}