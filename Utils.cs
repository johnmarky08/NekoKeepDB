using NekoKeepDB.Classes;
using System.Net.Mail;

namespace NekoKeepDB
{
    internal class Utils
    {
        private static readonly HashSet<string> AllowedAccountTypes = ["OAuth", "Custom"];
        public static string BCryptEncrypt(string text) => BCrypt.Net.BCrypt.HashPassword(text);

        public static bool BCryptVerify(string text, string hash) => BCrypt.Net.BCrypt.Verify(text, hash);

        public static bool IsAuthenticated()
        {
            if (User.Session == null)
            {
                ThrowError("User is not authenticated.");
                return false;
            }

            return true;
        }

        public static bool ValidateType(string type)
        {
            if (!AllowedAccountTypes.Contains(type))
            {
                ThrowError("Invalid Type! Only accepts 'OAuth' or 'Custom'");
                return false;
            }

            return true;
        }

        public static bool ValidateEmail(string email)
        {
            string trimmed = email.Trim();

            try
            {
                var addr = new MailAddress(trimmed);

                if (addr.Address != trimmed)
                {
                    ThrowError("Invalid email address format.");
                    return false;
                }

                return true;
            }
            catch
            {
                ThrowError("Invalid email address format.");
                return false;
            }
        }

        public static bool ValidatePassword(string password)
        {
            bool isValid = true;

            if (password.Length < 8)
            {
                ThrowError("Password must be at least 8 characters long.");
                isValid = false;
            }

            if (!password.Any(char.IsUpper))
            {
                ThrowError("Password must contain at least one uppercase letter.");
                isValid = false;
            }

            if (!password.Any(char.IsLower))
            {
                ThrowError("Password must contain at least one lowercase letter.");
                isValid = false;
            }

            if (!password.Any(char.IsDigit))
            {
                ThrowError("Password must contain at least one number.");
                isValid = false;
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ThrowError("Password must contain at least one special character.");
                isValid = false;
            }

            return isValid;
        }

        public static bool ValidateMpin(string mpin)
        {
            bool isValid = true;

            if (!int.TryParse(mpin, out _))
            {
                ThrowError("MPIN must contain only whole integers (0-9).");
                isValid = false;
            }

            if (mpin.Length != 6)
            {
                ThrowError("MPIN must be exactly 6 digits long.");
                isValid = false;
            }

            return isValid;
        }

        public static void ThrowError(string message)
        {
            Console.WriteLine(message);
        }
    }
}
