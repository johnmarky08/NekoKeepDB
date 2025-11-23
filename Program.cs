using NekoKeepDB.Classes;
using static System.Console;

namespace NekoKeepDB
{
    internal class Program
    {
        static void Main()
        {
            Database.Connect();
            Database.CreateAllTables();

            List<string> menu = ["Signup", "Signin"];
            bool exit = false;
            while (!exit)
            {
                WriteLine();
                WriteLine("=== MENU ===\n" + string.Join("\n", menu.Select(menuItem => $"[ {menu.IndexOf(menuItem) + 1} ] - {menuItem}")) + "\n[ 0 ] - Exit");
                Write("What do you want to test? ");

                int selected = int.Parse(ReadLine()!);
                WriteLine();
                switch (selected)
                {
                    case 0:
                        {
                            exit = true;
                            break;
                        }
                    case 1:
                        {
                            // ================================== Test User Creation ==================================
                            WriteLine("=== TEST USER CREATION ===");
                            Write("Enter User Display Name: ");
                            string displayName = ReadLine()!;

                            Write("Enter User Email: ");
                            string email = ReadLine()!;
                            Write("Enter User Password: ");
                            string password = ReadLine()!;
                            string encryptedPassword = Crypto.Encrypt(password);

                            Write("Enter User MPIN: ");
                            string mpin = ReadLine()!;
                            string encryptedMpin = Crypto.Encrypt(mpin);

                            Write("Enter Cat Theme Preset ID: ");
                            int catThemePresetId = int.Parse(ReadLine()!);

                            Database.CreateUser(displayName, email, encryptedPassword, encryptedMpin, catThemePresetId);
                            break;
                            // ================================== Test User Creation ==================================
                        }
                    case 2:
                        {
                            // =================================== Test User Login ===================================
                            WriteLine("=== TEST USER LOGIN ===");
                            Write("Enter User Email: ");
                            string email = ReadLine()!;
                            Write("Enter User Password: ");
                            string password = ReadLine()!;

                            bool user = Database.AuthenticateUser(email, password);
                            if (user)
                            {
                                WriteLine($"\n=== USER DEAILS ===\nID: {User.Id}\nDisplay Name: {User.DisplayName}\nEmail: {User.Email}\nCat Preset Id: {User.CatPresetId}");

                                Write("\n\nTest MPIN? (y, n): ");
                                if (string.Equals(ReadLine()!.ToLower(), "y"))
                                {
                                    Write("Enter User MPIN: ");
                                    string mpin = ReadLine()!;
                                    bool mpinValid = Crypto.Verify(mpin, User.EncryptedMpin);
                                    WriteLine(mpinValid ? "MPIN is correct!" : "MPIN is incorrect!");
                                }
                            }
                            break;
                            // =================================== Test User Login ===================================
                        }
                    default:
                        {
                            WriteLine("Invalid Selection!");
                            break;
                        }
                }
            }

            Database.Disconnect();
        }
    }
}
