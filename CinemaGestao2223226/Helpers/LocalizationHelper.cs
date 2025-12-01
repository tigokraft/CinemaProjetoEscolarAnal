using Microsoft.AspNetCore.Localization;

namespace CinemaGestao2223226.Helpers
{
    public static class LocalizationHelper
    {
        public static string GetCurrentLanguage(HttpContext context)
        {
            var requestCulture = context.Features.Get<IRequestCultureFeature>();
            return requestCulture?.RequestCulture.UICulture.Name ?? "pt-PT";
        }

        public static bool IsEnglish(HttpContext context)
        {
            return GetCurrentLanguage(context) == "en-US";
        }

        public static bool IsPortuguese(HttpContext context)
        {
            return GetCurrentLanguage(context) == "pt-PT";
        }
    }
}