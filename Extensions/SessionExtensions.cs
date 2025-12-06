// Extensions/SessionExtensions.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReverseMarket.Extensions
{
    public static class SessionExtensions
    {
        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetString("IsLoggedIn") == "true";
        }

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32("UserId");
        }

        public static string GetUserName(this ISession session)
        {
            return session.GetString("UserName") ?? "";
        }

        public static string GetUserType(this ISession session)
        {
            return session.GetString("UserType") ?? "";
        }

        public static bool IsSeller(this ISession session)
        {
            return session.GetString("UserType") == "Seller";
        }

        public static bool IsBuyer(this ISession session)
        {
            return session.GetString("UserType") == "Buyer";
        }
    }

    // Attribute للتحقق من تسجيل الدخول
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Session.IsLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
    }

    // Attribute للتحقق من نوع المستخدم
    public class RequireUserTypeAttribute : ActionFilterAttribute
    {
        private readonly string _requiredUserType;

        public RequireUserTypeAttribute(string requiredUserType)
        {
            _requiredUserType = requiredUserType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Session.IsLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var userType = context.HttpContext.Session.GetUserType();
            if (userType != _requiredUserType)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}