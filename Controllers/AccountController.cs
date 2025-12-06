// Controllers/AccountController.cs - النسخة المحدثة والمكملة
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.Services;
using System.Security.Claims;

namespace ReverseMarket.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        //private readonly IWhatsAppService _whatsAppService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AccountController> _logger;
        private readonly WhatsAppService _whats;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            //IWhatsAppService whatsAppService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AccountController> logger,
            WhatsAppService whats)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            //_whatsAppService = whatsAppService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _whats = whats;
        }

        async Task<int> RegisterUser()
        {
            var adminPhone = "+9647700227211";
            var adminUser = await _userManager.FindByNameAsync(adminPhone);

            adminUser = new ApplicationUser
            {
                UserName = adminPhone,
                PhoneNumber = adminPhone,
                Email = "user@reversemarket.iq",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                FirstName = "مدير",
                LastName = "النظام",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "ذكر",
                City = "بغداد",
                District = "الكرادة",
                UserType = UserType.Buyer,
                IsPhoneVerified = true,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(adminUser, "User@1234");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoginAsync()
        {
            //int operation = await RegisterUser();
            // Check if user is already logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                CountryCode = "+964"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cleanPhoneNumber = model.CountryCode + model.PhoneNumber.TrimStart('0');

                // Check for admin login
                if (cleanPhoneNumber == "+9647805006974")
                {
                    var adminOtp = GenerateOTP();
                    HttpContext.Session.SetString("AdminOTP", adminOtp);
                    HttpContext.Session.SetString("AdminPhone", cleanPhoneNumber);
                    HttpContext.Session.SetString("LoginType", "Admin");

                    //await _whatsAppService.SendOTPAsync(cleanPhoneNumber, adminOtp);
                    var request = new WhatsAppMessageRequest
                    {
                        recipient = cleanPhoneNumber,
                        message = $"كود تسجيل دخول الأدمن: {adminOtp}\nصالح لمدة 5 دقائق",
                        type = "whatsapp",  // ✅ إضافة النوع
                        lang = "ar",        // ✅ إضافة اللغة
                        sender_id = "AliJamal"  // ✅ إضافة المرسل
                    };

                    _logger.LogInformation("📤 إرسال كود للأدمن {Phone}: {Code}", cleanPhoneNumber, adminOtp);

                    var result = await _whats.SendMessageAsync(request);

                    if (result.Success)
                    {
                        _logger.LogInformation("✅ تم إرسال كود الأدمن بنجاح");
                        TempData["InfoMessage"] = "تم إرسال رمز التحقق للأدمن";
                    }
                    else
                    {
                        _logger.LogError("❌ فشل إرسال كود الأدمن: {Error}", result.Message);
                        TempData["ErrorMessage"] = "فشل إرسال رمز التحقق";
                    }

                    return RedirectToAction("VerifyAdminOTP");
                }

                // Find user by phone number
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == cleanPhoneNumber);

                if (user != null)
                {
                    // Check if account is active
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError("", "تم إيقاف هذا الحساب. يرجى التواصل مع الإدارة.");
                        return View(model);
                    }

                    if (user.PhoneNumberConfirmed)
                    {
                        // Verified user - send login OTP
                        var otp = GenerateOTP();

                        HttpContext.Session.SetString("OTP", otp);
                        HttpContext.Session.SetString("PhoneNumber", cleanPhoneNumber);
                        HttpContext.Session.SetString("LoginType", "ExistingVerifiedUser");
                        HttpContext.Session.SetString("UserId", user.Id);

                        //await _whatsAppService.SendLoginOTPAsync(cleanPhoneNumber, otp, user.FirstName);
                        var request = new WhatsAppMessageRequest
                        {
                            recipient = cleanPhoneNumber, // e.g. "+9647502171212"
                            message = otp
                        };

                        await _whats.SendMessageAsync(request);

                        TempData["InfoMessage"] = $"مرحباً {user.FirstName}! تم إرسال رمز الدخول إلى واتساب.";
                        return RedirectToAction("VerifyOTP");
                    }
                    else
                    {
                        // Unverified user - re-verify phone
                        var verificationCode = GenerateOTP();

                        HttpContext.Session.SetString("VerificationCode", verificationCode);
                        HttpContext.Session.SetString("PhoneNumber", cleanPhoneNumber);
                        HttpContext.Session.SetString("LoginType", "ExistingUnverifiedUser");
                        HttpContext.Session.SetString("UserId", user.Id);

                        //await _whatsAppService.SendPhoneVerificationAsync(cleanPhoneNumber, verificationCode, true);
                        var request = new WhatsAppMessageRequest
                        {
                            recipient = cleanPhoneNumber, // e.g. "+9647502171212"
                            message = verificationCode
                        };

                        await _whats.SendMessageAsync(request);
                        TempData["WarningMessage"] = "حسابك موجود لكن الهاتف غير مؤكد. يرجى تأكيد رقم الهاتف.";
                        return RedirectToAction("VerifyPhone");
                    }
                }
                else
                {
                    // New user - send verification code for registration
                    var verificationCode = GenerateOTP();

                    HttpContext.Session.SetString("VerificationCode", verificationCode);
                    HttpContext.Session.SetString("PhoneNumber", cleanPhoneNumber);
                    HttpContext.Session.SetString("LoginType", "NewUser");

                    //await _whatsAppService.SendPhoneVerificationAsync(cleanPhoneNumber, verificationCode, false);
                    var request = new WhatsAppMessageRequest
                    {
                        recipient = cleanPhoneNumber, // e.g. "+9647502171212"
                        message = verificationCode
                    };

                    await _whats.SendMessageAsync(request);

                    TempData["InfoMessage"] = "رقم جديد! يرجى تأكيد رقم الهاتف أولاً.";
                    return RedirectToAction("VerifyPhone");
                }
            }

            return View(model);
        }

        [HttpGet]
        //public IActionResult Login()
        //{
        //    // Check if user is already logged in
        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        if (User.IsInRole("Admin"))
        //        {
        //            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        //        }
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var model = new LoginViewModel
        //    {
        //        CountryCode = "+964"
        //    };
        //    return View(model);
        //}

        [HttpGet]
        public IActionResult VerifyAdminOTP()
        {
            var adminPhone = HttpContext.Session.GetString("AdminPhone");
            var loginType = HttpContext.Session.GetString("LoginType");

            if (string.IsNullOrEmpty(adminPhone) || loginType != "Admin")
            {
                TempData["ErrorMessage"] = "جلسة التحقق منتهية الصلاحية.";
                return RedirectToAction("Login");
            }

            ViewBag.PhoneNumber = adminPhone;
            return View(new VerifyOTPViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAdminOTP(VerifyOTPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var storedOTP = HttpContext.Session.GetString("AdminOTP");
                var adminPhone = HttpContext.Session.GetString("AdminPhone");
                var loginType = HttpContext.Session.GetString("LoginType");

                if (model.OTP == storedOTP && !string.IsNullOrEmpty(adminPhone) && loginType == "Admin")
                {
                    // Find admin user and sign in
                    var adminUser = await _userManager.FindByNameAsync(adminPhone);
                    if (adminUser != null)
                    {
                        await _signInManager.SignInAsync(adminUser, isPersistent: true);

                        // Clear verification session
                        ClearVerificationSession();

                        TempData["SuccessMessage"] = "مرحباً بك في لوحة التحكم!";
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                }

                ModelState.AddModelError("", "رمز التحقق غير صحيح");
            }

            ViewBag.PhoneNumber = HttpContext.Session.GetString("AdminPhone");
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var loginType = HttpContext.Session.GetString("LoginType");

            if (string.IsNullOrEmpty(phoneNumber) ||
                (loginType != "ExistingVerifiedUser" && loginType != "ExistingUnverifiedUser"))
            {
                TempData["ErrorMessage"] = "جلسة التحقق منتهية الصلاحية. يرجى المحاولة مرة أخرى.";
                return RedirectToAction("Login");
            }

            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.LoginType = loginType;
            return View(new VerifyOTPViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var storedOTP = HttpContext.Session.GetString("OTP");
                var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                var loginType = HttpContext.Session.GetString("LoginType");
                var userId = HttpContext.Session.GetString("UserId");

                if (model.OTP == storedOTP && !string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        if (loginType == "ExistingVerifiedUser")
                        {
                            // Sign in verified user مع isPersistent = true
                            await _signInManager.SignInAsync(user, isPersistent: true);

                            // إضافة الـ Claims للجلسة
                            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                        new Claim(ClaimTypes.GivenName, user.FirstName),
                        new Claim(ClaimTypes.Surname, user.LastName),
                        new Claim("UserType", user.UserType.ToString())
                    };

                            await _userManager.AddClaimsAsync(user, claims);

                            ClearVerificationSession();

                            TempData["SuccessMessage"] = $"مرحباً بعودتك {user.FirstName}!";
                            return RedirectToAction("Index", "Home");
                        }
                        else if (loginType == "ExistingUnverifiedUser")
                        {
                            // Confirm phone and sign in
                            user.PhoneNumberConfirmed = true;
                            var updateResult = await _userManager.UpdateAsync(user);

                            if (updateResult.Succeeded)
                            {
                                // Add claims
                                var claims = new List<Claim>
                        {
                            new Claim("PhoneNumberConfirmed", "true"),
                            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                            new Claim(ClaimTypes.GivenName, user.FirstName),
                            new Claim(ClaimTypes.Surname, user.LastName),
                            new Claim("UserType", user.UserType.ToString())
                        };

                                await _userManager.AddClaimsAsync(user, claims);

                                await _signInManager.SignInAsync(user, isPersistent: true);
                                ClearVerificationSession();

                                TempData["SuccessMessage"] = $"تم تأكيد حسابك بنجاح! مرحباً بك {user.FirstName}";
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }

                ModelState.AddModelError("", "رمز التحقق غير صحيح");
            }

            ViewBag.PhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            ViewBag.LoginType = HttpContext.Session.GetString("LoginType");
            return View(model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var storedOTP = HttpContext.Session.GetString("OTP");
        //        var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
        //        var loginType = HttpContext.Session.GetString("LoginType");
        //        var userId = HttpContext.Session.GetString("UserId");

        //        if (model.OTP == storedOTP && !string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(userId))
        //        {
        //            var user = await _userManager.FindByIdAsync(userId);
        //            if (user != null)
        //            {
        //                if (loginType == "ExistingVerifiedUser")
        //                {
        //                    // Sign in verified user
        //                    await _signInManager.SignInAsync(user, isPersistent: true);
        //                    ClearVerificationSession();

        //                    TempData["SuccessMessage"] = $"مرحباً بعودتك {user.FirstName}!";
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                else if (loginType == "ExistingUnverifiedUser")
        //                {
        //                    // Confirm phone and sign in
        //                    user.PhoneNumberConfirmed = true;
        //                    var updateResult = await _userManager.UpdateAsync(user);

        //                    if (updateResult.Succeeded)
        //                    {
        //                        // Add phone confirmation claim
        //                        await _userManager.AddClaimAsync(user, new Claim("PhoneNumberConfirmed", "true"));

        //                        await _signInManager.SignInAsync(user, isPersistent: true);
        //                        ClearVerificationSession();

        //                        TempData["SuccessMessage"] = $"تم تأكيد حسابك بنجاح! مرحباً بك {user.FirstName}";
        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                }
        //            }
        //        }

        //        ModelState.AddModelError("", "رمز التحقق غير صحيح");
        //    }

        //    ViewBag.PhoneNumber = HttpContext.Session.GetString("PhoneNumber");
        //    ViewBag.LoginType = HttpContext.Session.GetString("LoginType");
        //    return View(model);
        //}

        [HttpGet]
        public IActionResult VerifyPhone()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var loginType = HttpContext.Session.GetString("LoginType");

            if (string.IsNullOrEmpty(phoneNumber) ||
                (loginType != "NewUser" && loginType != "ExistingUnverifiedUser"))
            {
                TempData["ErrorMessage"] = "جلسة التحقق منتهية الصلاحية.";
                return RedirectToAction("Login");
            }

            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.LoginType = loginType;
            return View(new VerifyPhoneViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(VerifyPhoneViewModel model)
        {
            if (ModelState.IsValid)
            {
                var storedCode = HttpContext.Session.GetString("VerificationCode");
                var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                var loginType = HttpContext.Session.GetString("LoginType");

                if (model.VerificationCode == storedCode)
                {
                    if (loginType == "NewUser")
                    {
                        // Phone verified for new user - proceed to account creation
                        HttpContext.Session.SetString("PhoneVerified", "true");
                        return RedirectToAction("CreateAccount");
                    }
                    else if (loginType == "ExistingUnverifiedUser")
                    {
                        // Confirm existing user's phone
                        var userId = HttpContext.Session.GetString("UserId");
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var user = await _userManager.FindByIdAsync(userId);
                            if (user != null)
                            {
                                user.PhoneNumberConfirmed = true;
                                var result = await _userManager.UpdateAsync(user);

                                if (result.Succeeded)
                                {
                                    // Add phone confirmation claim
                                    await _userManager.AddClaimAsync(user, new Claim("PhoneNumberConfirmed", "true"));

                                    await _signInManager.SignInAsync(user, isPersistent: true);
                                    ClearVerificationSession();

                                    TempData["SuccessMessage"] = $"تم تأكيد رقم هاتفك! مرحباً بك {user.FirstName}";
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                }

                ModelState.AddModelError("", "رمز التحقق غير صحيح");
            }

            ViewBag.PhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            ViewBag.LoginType = HttpContext.Session.GetString("LoginType");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccount()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var phoneVerified = HttpContext.Session.GetString("PhoneVerified");
            var loginType = HttpContext.Session.GetString("LoginType");

            if (string.IsNullOrEmpty(phoneNumber) || phoneVerified != "true" || loginType != "NewUser")
            {
                TempData["ErrorMessage"] = "جلسة التحقق منتهية الصلاحية.";
                return RedirectToAction("Login");
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.PhoneNumber = phoneNumber;

            return View(new CreateAccountViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                var phoneVerified = HttpContext.Session.GetString("PhoneVerified");
                var loginType = HttpContext.Session.GetString("LoginType");

                if (string.IsNullOrEmpty(phoneNumber) || phoneVerified != "true" || loginType != "NewUser")
                {
                    ModelState.AddModelError("", "جلسة التحقق منتهية الصلاحية");
                    return RedirectToAction("Login");
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(phoneNumber);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "هذا الرقم مسجل مسبقاً");
                    return RedirectToAction("Login");
                }

                // Check email uniqueness if provided
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailExists = await _userManager.FindByEmailAsync(model.Email);
                    if (emailExists != null)
                    {
                        ModelState.AddModelError("Email", "هذا البريد الإلكتروني مستخدم مسبقاً");
                        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                        return View(model);
                    }
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = phoneNumber,
                    PhoneNumber = phoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    City = model.City,
                    District = model.District,
                    Location = model.Location,
                    Email = model.Email,
                    UserType = model.UserType,
                    StoreName = model.StoreName,
                    StoreDescription = model.StoreDescription,
                    WebsiteUrl1 = model.WebsiteUrl1,
                    WebsiteUrl2 = model.WebsiteUrl2,
                    WebsiteUrl3 = model.WebsiteUrl3,
                    PhoneNumberConfirmed = true,
                    IsPhoneVerified = true,
                    IsActive = true,
                    // إضافة حقل الموافقة على المتجر
                    IsStoreApproved = model.UserType == UserType.Buyer ? true : false, // المشترون تلقائي، البائعون يحتاجون موافقة
                    CreatedAt = DateTime.Now
                };

                // Save profile image if provided
                if (model.ProfileImage != null)
                {
                    var imagePath = await SaveProfileImageAsync(model.ProfileImage);
                    user.ProfileImage = imagePath;
                }

                // Create user with a temporary password
                var result = await _userManager.CreateAsync(user, "TempPassword@123");

                if (result.Succeeded)
                {
                    // Assign role based on user type
                    var roleName = model.UserType == UserType.Seller ? "Seller" : "Buyer";
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Add claims
                    var claims = new List<Claim>
            {
                new Claim("PhoneNumberConfirmed", "true"),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("UserType", user.UserType.ToString())
            };

                    await _userManager.AddClaimsAsync(user, claims);
                    if (model.UserType == UserType.Seller && !string.IsNullOrWhiteSpace(model.StoreCategories))
                    {
                        try
                        {
                            var categoryIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(model.StoreCategories);

                            if (categoryIds != null && categoryIds.Any())
                            {
                                foreach (var subCategory2Id in categoryIds)
                                {
                                    var subCategory2 = await _context.SubCategories2
                                        .Include(sc2 => sc2.SubCategory1)
                                        .FirstOrDefaultAsync(sc2 => sc2.Id == subCategory2Id);

                                    if (subCategory2 != null)
                                    {
                                        var storeCategory = new StoreCategory
                                        {
                                            UserId = user.Id,
                                            CategoryId = subCategory2.SubCategory1.CategoryId,
                                            SubCategory1Id = subCategory2.SubCategory1Id,
                                            SubCategory2Id = subCategory2.Id,
                                            CreatedAt = DateTime.Now
                                        };
                                        _context.StoreCategories.Add(storeCategory);
                                    }
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "خطأ في معالجة فئات المتجر");
                        }
                    }

                    // Add store categories for sellers
                    //////////if (model.UserType == UserType.Seller && model.StoreCategories?.Any() == true)
                    //////////{
                    //////////    foreach (var subCategory2IdStr in model.StoreCategories)
                    //////////    {
                    //////////        if (int.TryParse(subCategory2IdStr.ToString(), out int subCategory2Id))
                    //////////        {
                    //////////            // جلب SubCategory2 للحصول على المعلومات الكاملة
                    //////////            var subCategory2 = await _context.SubCategories2
                    //////////                .Include(sc2 => sc2.SubCategory1)
                    //////////                .FirstOrDefaultAsync(sc2 => sc2.Id == subCategory2Id);

                    //////////            if (subCategory2 != null)
                    //////////            {
                    //////////                var storeCategory = new StoreCategory
                    //////////                {
                    //////////                    UserId = user.Id,
                    //////////                    CategoryId = subCategory2.SubCategory1.CategoryId,
                    //////////                    SubCategory1Id = subCategory2.SubCategory1Id,
                    //////////                    SubCategory2Id = subCategory2.Id,
                    //////////                    CreatedAt = DateTime.Now
                    //////////                };
                    //////////                _context.StoreCategories.Add(storeCategory);
                    //////////            }
                    //////////        }
                    //////////    }
                    //////////    await _context.SaveChangesAsync();
                    //////////}

                    //if (model.UserType == UserType.Seller && model.StoreCategories?.Any() == true)
                    //{
                    //    foreach (var categoryId in model.StoreCategories)
                    //    {
                    //        var storeCategory = new StoreCategory
                    //        {
                    //            UserId = user.Id,
                    //            CategoryId = categoryId,
                    //            CreatedAt = DateTime.Now
                    //        };
                    //        _context.StoreCategories.Add(storeCategory);
                    //    }
                    //    await _context.SaveChangesAsync();
                    //}

                    // Sign in the user مع isPersistent = true للحفاظ على الجلسة
                    await _signInManager.SignInAsync(user, isPersistent: true);

                    // Clear session
                    ClearVerificationSession();

                    // رسالة مختلفة حسب نوع المستخدم
                    if (model.UserType == UserType.Seller)
                    {
                        TempData["WarningMessage"] = $"مرحباً بك {user.FirstName}! تم إنشاء حسابك بنجاح. متجرك قيد المراجعة من الإدارة.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"مرحباً بك {user.FirstName}! تم إنشاء حسابك بنجاح";
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
        //        var phoneVerified = HttpContext.Session.GetString("PhoneVerified");
        //        var loginType = HttpContext.Session.GetString("LoginType");

        //        if (string.IsNullOrEmpty(phoneNumber) || phoneVerified != "true" || loginType != "NewUser")
        //        {
        //            ModelState.AddModelError("", "جلسة التحقق منتهية الصلاحية");
        //            return RedirectToAction("Login");
        //        }

        //        // Check if user already exists
        //        var existingUser = await _userManager.FindByNameAsync(phoneNumber);
        //        if (existingUser != null)
        //        {
        //            ModelState.AddModelError("", "هذا الرقم مسجل مسبقاً");
        //            return RedirectToAction("Login");
        //        }

        //        // Check email uniqueness if provided
        //        if (!string.IsNullOrEmpty(model.Email))
        //        {
        //            var emailExists = await _userManager.FindByEmailAsync(model.Email);
        //            if (emailExists != null)
        //            {
        //                ModelState.AddModelError("Email", "هذا البريد الإلكتروني مستخدم مسبقاً");
        //                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        //                return View(model);
        //            }
        //        }

        //        // Create new user
        //        var user = new ApplicationUser
        //        {
        //            UserName = phoneNumber,
        //            PhoneNumber = phoneNumber,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            DateOfBirth = model.DateOfBirth,
        //            Gender = model.Gender,
        //            City = model.City,
        //            District = model.District,
        //            Location = model.Location,
        //            Email = model.Email,
        //            UserType = model.UserType,
        //            StoreName = model.StoreName,
        //            StoreDescription = model.StoreDescription,
        //            WebsiteUrl1 = model.WebsiteUrl1,
        //            WebsiteUrl2 = model.WebsiteUrl2,
        //            WebsiteUrl3 = model.WebsiteUrl3,
        //            PhoneNumberConfirmed = true,
        //            IsPhoneVerified = true,
        //            IsActive = true,
        //            CreatedAt = DateTime.Now
        //        };

        //        // Save profile image if provided
        //        if (model.ProfileImage != null)
        //        {
        //            var imagePath = await SaveProfileImageAsync(model.ProfileImage);
        //            user.ProfileImage = imagePath;
        //        }

        //        // Create user with a temporary password (will be changed to OTP-based later)
        //        var result = await _userManager.CreateAsync(user, "TempPassword@123");

        //        if (result.Succeeded)
        //        {
        //            // Assign role based on user type
        //            var roleName = model.UserType == UserType.Seller ? "Seller" : "Buyer";
        //            await _userManager.AddToRoleAsync(user, roleName);

        //            // Add phone confirmation claim
        //            await _userManager.AddClaimAsync(user, new Claim("PhoneNumberConfirmed", "true"));

        //            // Add store categories for sellers
        //            if (model.UserType == UserType.Seller && model.StoreCategories?.Any() == true)
        //            {
        //                foreach (var categoryId in model.StoreCategories)
        //                {
        //                    var storeCategory = new StoreCategory
        //                    {
        //                        UserId = user.Id,
        //                        CategoryId = categoryId,
        //                        CreatedAt = DateTime.Now
        //                    };
        //                    _context.StoreCategories.Add(storeCategory);
        //                }
        //                await _context.SaveChangesAsync();
        //            }

        //            // Sign in the user
        //            await _signInManager.SignInAsync(user, isPersistent: true);

        //            // Send welcome message
        //            //await _whatsAppService.SendWelcomeMessageAsync(phoneNumber, user.FirstName, user.UserType.ToString());

        //            // Clear session
        //            ClearVerificationSession();

        //            TempData["SuccessMessage"] = $"مرحباً بك {user.FirstName}! تم إنشاء حسابك بنجاح";
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            foreach (var error in result.Errors)
        //            {
        //                ModelState.AddModelError("", error.Description);
        //            }
        //        }
        //    }

        //    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        //    return View(model);
        //}

        [HttpPost]
        public async Task<IActionResult> ResendCode()
        {
            try
            {
                var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                var adminPhone = HttpContext.Session.GetString("AdminPhone");
                var loginType = HttpContext.Session.GetString("LoginType");

                // Handle admin resend
                if (loginType == "Admin" && !string.IsNullOrEmpty(adminPhone))
                {
                    var newAdminOtp = GenerateOTP();
                    HttpContext.Session.SetString("AdminOTP", newAdminOtp);
                    // await _whatsAppService.SendOTPAsync(adminPhone, newAdminOtp);
                    var request = new WhatsAppMessageRequest
                    {
                        recipient = adminPhone, // e.g. "+9647502171212"
                        message = newAdminOtp
                    };

                    await _whats.SendMessageAsync(request);
                    return Json(new { success = true, message = "تم إعادة إرسال رمز التحقق للأدمن" });
                }

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return Json(new { success = false, message = "جلسة منتهية الصلاحية" });
                }

                var newCode = GenerateOTP();

                if (loginType == "ExistingVerifiedUser")
                {
                    // Resend login OTP
                    HttpContext.Session.SetString("OTP", newCode);
                    var userId = HttpContext.Session.GetString("UserId");

                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            // await _whatsAppService.SendLoginOTPAsync(phoneNumber, newCode, user.FirstName);
                            var request = new WhatsAppMessageRequest
                            {
                                recipient = phoneNumber, // e.g. "+9647502171212"
                                message = newCode
                            };

                            await _whats.SendMessageAsync(request);
                        }
                        else
                        {
                            //await _whatsAppService.SendOTPAsync(phoneNumber, newCode);
                            var request = new WhatsAppMessageRequest
                            {
                                recipient = phoneNumber, // e.g. "+9647502171212"
                                message = newCode
                            };

                            await _whats.SendMessageAsync(request);
                        }
                    }
                }
                else
                {
                    // Resend verification code
                    HttpContext.Session.SetString("VerificationCode", newCode);
                    bool isExistingUser = loginType == "ExistingUnverifiedUser";
                    //await _whatsAppService.SendPhoneVerificationAsync(phoneNumber, newCode, isExistingUser);
                    var request = new WhatsAppMessageRequest
                    {
                        recipient = phoneNumber, // e.g. "+9647502171212"
                        message = newCode
                    };

                    await _whats.SendMessageAsync(request);
                }

                return Json(new { success = true, message = "تم إعادة إرسال الرمز بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إعادة الإرسال");
                return Json(new { success = false, message = "حدث خطأ في إعادة الإرسال" });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["InfoMessage"] = "تم تسجيل الخروج بنجاح";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Private Methods

        private void ClearVerificationSession()
        {
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("PhoneNumber");
            HttpContext.Session.Remove("LoginType");
            HttpContext.Session.Remove("PhoneVerified");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("AdminOTP");
            HttpContext.Session.Remove("AdminPhone");
        }

        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }


        private async Task<string?> SaveProfileImageAsync(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return null;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return null;

                if (image.Length > 5 * 1024 * 1024)
                    return null;

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"/uploads/profiles/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حفظ صورة الملف الشخصي");
                return null;
            }
        }

        #endregion

        #region Ali testing the chat app : 
        //public async Task<IActionResult> AdminLogin()
        //{
        //    var user = await _userManager.FindByNameAsync("+9647805006974");
        //    await _signInManager.PasswordSignInAsync(user, "Admin@1234", false, false);
        //    return RedirectToAction("Index", "Home");
        //}

        //public async Task<IActionResult> UserLogin()
        //{
        //    var user = await _userManager.FindByNameAsync("+9647700227211");
        //    await _signInManager.PasswordSignInAsync(user, "User@1234", false, false);
        //    return RedirectToAction("Index", "Home");
        //}

        #endregion
    }
}