using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models.Identity;
using ReverseMarket.SignalR;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatController> logger)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// صفحة إدارة المحادثات - عرض جميع المحادثات في النظام
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // ✅ الحل: جلب جميع الرسائل أولاً ثم المعالجة في الذاكرة
                var allMessages = await _context.ChatMessages
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                if (!allMessages.Any())
                {
                    TempData["ErrorMessage"] = "لا توجد محادثات في النظام";
                    return View(new List<AdminConversationViewModel>());
                }

                // تجميع المحادثات في الذاكرة (بدلاً من SQL)
                var conversations = allMessages
                    .GroupBy(m => new
                    {
                        User1 = string.Compare(m.SenderId, m.ReceiverId, StringComparison.Ordinal) < 0
                            ? m.SenderId : m.ReceiverId,
                        User2 = string.Compare(m.SenderId, m.ReceiverId, StringComparison.Ordinal) < 0
                            ? m.ReceiverId : m.SenderId
                    })
                    .Select(g => new
                    {
                        User1 = g.Key.User1,
                        User2 = g.Key.User2,
                        LastMessage = g.OrderByDescending(m => m.SentAt).First(),
                        MessageCount = g.Count(),
                        UnreadCount = g.Count(m => !m.IsRead)
                    })
                    .OrderByDescending(c => c.LastMessage.SentAt)
                    .ToList();

                // جلب معلومات المستخدمين
                var userIds = conversations
                    .SelectMany(c => new[] { c.User1, c.User2 })
                    .Distinct()
                    .ToList();

                var users = await _userManager.Users
                    .Where(u => userIds.Contains(u.UserName))
                    .ToDictionaryAsync(u => u.UserName, u => u);

                var conversationViewModels = conversations.Select(c => new AdminConversationViewModel
                {
                    User1Id = c.User1,
                    User2Id = c.User2,
                    User1 = users.GetValueOrDefault(c.User1),
                    User2 = users.GetValueOrDefault(c.User2),
                    LastMessage = c.LastMessage,
                    MessageCount = c.MessageCount,
                    UnreadCount = c.UnreadCount
                }).ToList();

                return View(conversationViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب المحادثات");
                TempData["ErrorMessage"] = $"حدث خطأ في جلب المحادثات: {ex.Message}";
                return View(new List<AdminConversationViewModel>());
            }
        }

        /// <summary>
        /// عرض محادثة محددة للإدارة
        /// </summary>
        public async Task<IActionResult> ViewConversation(string user1, string user2)
        {
            try
            {
                if (string.IsNullOrEmpty(user1) || string.IsNullOrEmpty(user2))
                {
                    TempData["ErrorMessage"] = "معرفات المستخدمين غير صحيحة";
                    return RedirectToAction("Index");
                }

                // جلب معلومات المستخدمين
                var userInfo1 = await _userManager.FindByNameAsync(user1);
                var userInfo2 = await _userManager.FindByNameAsync(user2);

                if (userInfo1 == null || userInfo2 == null)
                {
                    TempData["ErrorMessage"] = "المستخدمون غير موجودون";
                    return RedirectToAction("Index");
                }

                // جلب الرسائل
                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == user1 && m.ReceiverId == user2) ||
                               (m.SenderId == user2 && m.ReceiverId == user1))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var viewModel = new AdminChatViewModel
                {
                    User1 = userInfo1,
                    User2 = userInfo2,
                    Messages = messages,
                    CurrentAdminId = User.Identity?.Name ?? string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في عرض المحادثة بين {User1} و {User2}", user1, user2);
                TempData["ErrorMessage"] = "حدث خطأ في عرض المحادثة";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// إرسال رسالة من الإدارة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAdminMessage(string receiverId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
                {
                    return Json(new { success = false, message = "البيانات غير مكتملة" });
                }

                var adminId = User.Identity?.Name ?? string.Empty;
                var receiver = await _userManager.FindByNameAsync(receiverId);

                if (receiver == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                // إنشاء الرسالة
                var chatMessage = new ChatMessage
                {
                    SenderId = adminId,
                    ReceiverId = receiverId,
                    Message = $"[رسالة من الإدارة] {message}",
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                // إرسال الرسالة عبر SignalR
                await _hubContext.Clients.User(receiverId)
                    .SendAsync("ReceiveMessage", adminId, chatMessage.Message,
                              chatMessage.SentAt.ToString("HH:mm"), null, null);

                _logger.LogInformation("تم إرسال رسالة من الإدارة {AdminId} إلى {ReceiverId}", adminId, receiverId);

                return Json(new { success = true, message = "تم إرسال الرسالة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال رسالة الإدارة");
                return Json(new { success = false, message = "حدث خطأ في إرسال الرسالة" });
            }
        }

        /// <summary>
        /// حذف محادثة (للإدارة فقط)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversation(string user1, string user2)
        {
            try
            {
                if (string.IsNullOrEmpty(user1) || string.IsNullOrEmpty(user2))
                {
                    return Json(new { success = false, message = "معرفات المستخدمين غير صحيحة" });
                }

                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == user1 && m.ReceiverId == user2) ||
                               (m.SenderId == user2 && m.ReceiverId == user1))
                    .ToListAsync();

                if (messages.Any())
                {
                    _context.ChatMessages.RemoveRange(messages);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("تم حذف المحادثة بين {User1} و {User2} بواسطة الإدارة {AdminId}",
                                         user1, user2, User.Identity?.Name ?? "Unknown");
                }

                return Json(new { success = true, message = "تم حذف المحادثة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المحادثة بين {User1} و {User2}", user1, user2);
                return Json(new { success = false, message = "حدث خطأ في حذف المحادثة" });
            }
        }

        /// <summary>
        /// إحصائيات المحادثات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChatStatistics()
        {
            try
            {
                // جلب جميع الرسائل للمعالجة في الذاكرة
                var allMessages = await _context.ChatMessages.ToListAsync();

                var uniqueConversations = allMessages
                    .GroupBy(m => new
                    {
                        User1 = string.Compare(m.SenderId, m.ReceiverId, StringComparison.Ordinal) < 0
                            ? m.SenderId : m.ReceiverId,
                        User2 = string.Compare(m.SenderId, m.ReceiverId, StringComparison.Ordinal) < 0
                            ? m.ReceiverId : m.SenderId
                    })
                    .Count();

                var todayMessages = allMessages
                    .Count(m => m.SentAt.Date == DateTime.Today);

                var activeUsersToday = allMessages
                    .Where(m => m.SentAt.Date == DateTime.Today)
                    .Select(m => m.SenderId)
                    .Distinct()
                    .Count();

                var stats = new
                {
                    TotalConversations = uniqueConversations,
                    TotalMessages = allMessages.Count,
                    TodayMessages = todayMessages,
                    ActiveUsersToday = activeUsersToday
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب إحصائيات المحادثات");
                return Json(new { error = "حدث خطأ في جلب الإحصائيات" });
            }
        }
    }

    // ViewModels للإدارة
    public class AdminConversationViewModel
    {
        public string User1Id { get; set; } = string.Empty;
        public string User2Id { get; set; } = string.Empty;
        public ApplicationUser? User1 { get; set; }
        public ApplicationUser? User2 { get; set; }
        public ChatMessage? LastMessage { get; set; }
        public int MessageCount { get; set; }
        public int UnreadCount { get; set; }
    }

    public class AdminChatViewModel
    {
        public ApplicationUser User1 { get; set; } = null!;
        public ApplicationUser User2 { get; set; } = null!;
        public List<ChatMessage> Messages { get; set; } = new();
        public string CurrentAdminId { get; set; } = string.Empty;
    }
}