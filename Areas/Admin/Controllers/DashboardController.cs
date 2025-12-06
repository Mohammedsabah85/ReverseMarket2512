using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Areas.Admin.Models;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;


namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalRequests = await _context.Requests.CountAsync(),
                PendingRequests = await _context.Requests.CountAsync(r => r.Status == RequestStatus.Pending),
                TotalStores = await _context.Users.CountAsync(u => u.UserType == UserType.Seller &&
                                                                  !string.IsNullOrEmpty(u.StoreName)),
                TotalCategories = await _context.Categories.CountAsync(c => c.IsActive),
                RecentRequests = await _context.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}