using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models.Identity;
using ReverseMarket.SignalR;

namespace ReverseMarket.Controllers
{
    [Authorize] // ✅ التأكد من أن المستخدم مسجل دخول
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // ✅ صفحة المحادثات الخاصة بالمستخدم - واجهة منفصلة للمستخدمين العاديين
        public async Task<IActionResult> Index(string withUser)
        {
            // التحقق من صحة البيانات
            if (string.IsNullOrEmpty(withUser) || User.Identity.Name.ToLower().Equals(withUser.ToLower()))
                return RedirectToAction(nameof(MyChatting));

            // جلب بيانات المرسل والمستقبل
            var senderFullName = await _userManager.FindByNameAsync(User.Identity.Name.ToLower());
            string receiverShaping = withUser;

            // تطبيع رقم الهاتف للمستقبل
            int index = receiverShaping.IndexOf("964");
            string result = index >= 0 ? receiverShaping.Substring(index) : receiverShaping;
            receiverShaping = $"+{result.Trim().ToLower()}";

            var receiverFullName = await _userManager.FindByNameAsync(receiverShaping);

            if (receiverFullName == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(MyChatting));
            }

            // التحقق من أن المستخدم ليس أدمن - إذا كان أدمن يذهب للواجهة الإدارية
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Chat", new { area = "Admin", withUser = withUser });
            }

            var members = new ChatMembersDto
            {
                SenderId = User.Identity.Name.ToLower(),
                ReceiverId = withUser.ToLower(),
                SenderFullName = senderFullName.FirstName + " " + senderFullName.LastName,
                ReceiverFullName = receiverFullName.FirstName + " " + receiverFullName.LastName
            };

            // استخدام Layout العادي للمستخدمين العاديين
            ViewBag.UseAdminLayout = false;
            return View(members);
        }

        // Ajax endpoint to load messages
        [HttpGet]
        public IActionResult LoadMessages(string withUser, int skip = 0, int take = 10)
        {
            var currentUser = User.Identity.Name.ToLower();
            // cuz withuser comming like this "&#x2B;9647700227210"
            int index = withUser.IndexOf("964");

            string result = index >= 0 ? withUser.Substring(index) : withUser;
            // then shaping to interact with db
            withUser = $"+{result.Trim().ToLower()}";

            // if sender and reviever are the same, redirect to MyChatting
            if (currentUser == withUser)
                return RedirectToAction(nameof(MyChatting));

            var messages = _context.ChatMessages
                .Where(m => (m.SenderId == currentUser && m.ReceiverId == withUser) ||
                            (m.SenderId == withUser && m.ReceiverId == currentUser))
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToList()
                .OrderBy(m => m.SentAt) // reverse back to normal order
                .ToList();

            return PartialView("_ChatMessagesPartial", messages);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string receiver)
        {
            var sender = User.Identity.Name.ToLower();
            // cuz receiver comming like this "&#x2B;9647700227210"
            int index = receiver.IndexOf("964");

            string result = index >= 0 ? receiver.Substring(index) : receiver;
            receiver = $"+{result?.Trim().ToLower()}";

            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(receiver))
                return BadRequest("Invalid file upload.");

            // ✅ Sort usernames alphabetically to ensure unique folder per chat pair
            var users = new[] { sender, receiver };
            Array.Sort(users);
            var folderName = $"{users[0]}_{users[1]}";

            var uploadsFolder = Path.Combine("wwwroot", "chatfiles", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var fileUrl = $"/chatfiles/{folderName}/{uniqueFileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var chatMessage = new ChatMessage
            {
                SenderId = sender,
                ReceiverId = receiver,
                Message = "[File]",
                SentAt = DateTime.Now,
                FilePath = fileUrl,
                FileType = fileExtension,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send file to both sender and receiver
            await _hubContext.Clients.User(receiver)
                .SendAsync("ReceiveMessage", sender, "[File]", chatMessage.SentAt.ToString("HH:mm"), fileUrl, fileExtension);

            await _hubContext.Clients.User(sender)
                .SendAsync("ReceiveMessage", sender, "[File]", chatMessage.SentAt.ToString("HH:mm"), fileUrl, fileExtension);

            return Ok();
        }

        // ✅ صفحة قائمة المحادثات - للمستخدمين العاديين فقط
        public async Task<IActionResult> MyChatting()
        {
            var currentUser = User.Identity.Name.ToLower();

            var users = await (
             from m in _context.ChatMessages
             where m.SenderId == currentUser || m.ReceiverId == currentUser
             let otherUserId = m.SenderId == currentUser ? m.ReceiverId : m.SenderId
             join u in _context.Users on otherUserId equals u.UserName
             select new UnReadMessagesDto
             {
                 UserName = u.UserName,
                 FirstName = u.FirstName,
                 LastName = u.LastName
             }
             )
             .Distinct()
             .ToListAsync();

            return View(users); // A list to click chat with any user
        }
    }

    // ✅ Models للمحادثات
    public class ChatMembersDto
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderFullName { get; set; }
        public string ReceiverFullName { get; set; }
    }

    public class UnReadMessagesDto
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}