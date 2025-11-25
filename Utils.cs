using System.Net.Mail;

namespace NekoKeepDB
{

    internal class Utils
    {
        public static bool ValidateEmail(string email)
        {
            string trimmed = email.Trim();

            try
            {
                var addr = new MailAddress(trimmed);

                if (addr.Address != trimmed)
                {
                    Console.WriteLine("Invalid email address format.");
                    return false;
                }

                return true;
            }
            catch
            {
                Console.WriteLine("Invalid email address format.");
                return false;
            }
        }

        public static bool ValidatePassword(string password)
        {
            bool isValid = true;

            if (password.Length < 8)
            {
                Console.WriteLine("Password must be at least 8 characters long.");
                isValid = false;
            }

            if (!password.Any(char.IsUpper))
            {
                Console.WriteLine("Password must contain at least one uppercase letter.");
                isValid = false;
            }

            if (!password.Any(char.IsLower))
            {
                Console.WriteLine("Password must contain at least one lowercase letter.");
                isValid = false;
            }

            if (!password.Any(char.IsDigit))
            {
                Console.WriteLine("Password must contain at least one number.");
                isValid = false;
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                Console.WriteLine("Password must contain at least one special character.");
                isValid = false;
            }

            return isValid;
        }

        public static bool ValidateMpin(string mpin)
        {
            bool isValid = true;

            if (!int.TryParse(mpin, out _))
            {
                Console.WriteLine("MPIN must contain only whole integers (0-9).");
                isValid = false;
            }

            if (mpin.Length != 6)
            {
                Console.WriteLine("MPIN must be exactly 6 digits long.");
                isValid = false;
            }

            return isValid;
        }
    }
}
