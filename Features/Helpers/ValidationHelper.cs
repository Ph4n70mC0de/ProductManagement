using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ProductManagement.Features.Helpers
{
    public static class ValidationHelper
    {
        public static void ValidateRequiredString(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} is required");
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static void ValidateEmail(string? email, string fieldName)
        {
            ValidateRequiredString(email, fieldName);
            if (!IsValidEmail(email))
                throw new ValidationException($"{fieldName} has an invalid email format");
        }
    }
}