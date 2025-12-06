using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Areas.Admin.Models;
using ReverseMarket.Extensions;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string search, int? userType, bool? isActive, int page = 1)
        {
            var pageSize = 20;

            // جلب جميع المستخدمين من Identity
            var query = _userManager.Users.AsQueryable();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.PhoneNumber.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (userType.HasValue)
            {
                var userTypeEnum = (UserType)userType.Value;
                query = query.Where(u => u.UserType == userTypeEnum);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // حساب الإحصائيات الصحيحة
            var allUsers = await _userManager.Users.ToListAsync();

            var totalUsers = allUsers.Count;
            var activeUsers = allUsers.Count(u => u.IsActive);
            var inactiveUsers = allUsers.Count(u => !u.IsActive);
            var buyersCount = allUsers.Count(u => u.UserType == UserType.Buyer);
            var sellersCount = allUsers.Count(u => u.UserType == UserType.Seller);

            // تطبيق الترتيب والصفحات
            var totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // تحويل إلى UserViewModel
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    City = user.City,
                    District = user.District,
                    IsActive = user.IsActive,
                    IsPhoneVerified = user.IsPhoneVerified,
                    UserType = user.UserType,
                    StoreName = user.StoreName,
                    ProfileImage = user.ProfileImage,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            var viewModel = new AdminUsersViewModel
            {
                Users = userViewModels,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Search = search,
                UserTypeFilter = userType.HasValue ? (UserType?)userType.Value : null,
                IsActiveFilter = isActive,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                BuyersCount = buyersCount,
                SellersCount = sellersCount
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            //var roles = await _userManager.GetRolesAsync(user);
            //var storeCategories = user.StoreCategories?.Select(sc => sc.Category?.Name ?? "").ToList();
            //var userViewModel = UserViewModel.FromApplicationUser(user, roles, storeCategories);

            //return View(userViewModel);
            var userModel = User.FromApplicationUser(user);

            return View(userModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var storeCategories = user.StoreCategories?.Select(sc => sc.Category?.Name ?? "").ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                UserType = user.UserType,
                City = user.City,
                District = user.District,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                StoreName = user.StoreName,
                StoreDescription = user.StoreDescription,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,
                IsPhoneVerified = user.IsPhoneVerified,
                IsEmailVerified = user.IsEmailVerified,
                IsStoreApproved = user.IsStoreApproved,
                CurrentStoreCategories = storeCategories,
                SelectedRoles = roles.ToList(),
                AvailableRoles = allRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
  
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // ✅ Update basic info - مع التعامل مع null
            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.UserName = model.UserName ?? user.UserName;
            user.Email = model.Email ?? user.Email;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UserType = model.UserType;
            user.City = model.City ?? user.City;
            user.District = model.District ?? user.District;
            user.Location = model.Location;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender ?? user.Gender;
            user.StoreName = model.StoreName;
            user.StoreDescription = model.StoreDescription;
            user.WebsiteUrl1 = model.WebsiteUrl1;
            user.WebsiteUrl2 = model.WebsiteUrl2;
            user.WebsiteUrl3 = model.WebsiteUrl3;
            user.IsPhoneVerified = model.IsPhoneVerified;
            user.IsEmailVerified = model.IsEmailVerified;
            user.IsStoreApproved = model.IsStoreApproved;
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            // ✅ Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>()).ToList();
            var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            TempData["Success"] = "تم تحديث المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "المستخدم غير موجود" });
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = System.DateTime.Now;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Json(new { success = true, isActive = user.IsActive });
            }

            return Json(new { success = false, message = "فشل تحديث حالة المستخدم" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "المستخدم غير موجود" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });
            }

            return Json(new { success = false, message = "فشل حذف المستخدم" });
        }
    }
}