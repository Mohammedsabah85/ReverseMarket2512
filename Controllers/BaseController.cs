using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;

namespace ReverseMarket.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Load site settings for all pages
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            ViewBag.SiteSettings = siteSettings;

            await next();
        }

        // Helper method to get current user ID
        protected string? GetCurrentUserId()
        {
            return User.Identity?.IsAuthenticated == true
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;
        }

        // Helper method to get current user
        protected async Task<ReverseMarket.Models.Identity.ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Users.FindAsync(userId);
        }

        // Helper method to check if user is in role
        protected bool IsInRole(string roleName)
        {
            return User.IsInRole(roleName);
        }

        // Helper method to check if user is admin
        protected bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // Helper method to check if user is seller
        protected bool IsSeller()
        {
            return User.IsInRole("Seller");
        }

        // Helper method to check if user is buyer
        protected bool IsBuyer()
        {
            return User.IsInRole("Buyer");
        }
    }
}
