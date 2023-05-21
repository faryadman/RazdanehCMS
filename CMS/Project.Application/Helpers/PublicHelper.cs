using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Project.Application.Helpers
{
    public static class PublicHelper
    {
        public const string SessionCaptcha = "_Captcha";

        public const string RequiredValidationErrorMessage = "{0} را وارد نکرده اید";
        public const string NotValidValidationErrorMessage = "{0} نامعتبر است";
        public const string PhoneValidationErrorMessage = "شمراه همراه نامعتبر است";

        private static readonly Random random = new();

        public static int GetRandomInt()
        {
            int from = 11111, to = 99999;
            return random.Next(from, to);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string GetDisplayAttributeFrom(this Enum enumValue)
        {
            MemberInfo info = enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .First();
            if (info != null && info.CustomAttributes.Any())
            {
                DisplayAttribute nameAttr = info.GetCustomAttribute<DisplayAttribute>();
                return nameAttr != null ? nameAttr.Name : enumValue.ToString();
            }
            return enumValue.ToString();
        }
    }
}