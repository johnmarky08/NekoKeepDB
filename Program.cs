using NekoKeepDB.Classes;
using static System.Console;

namespace NekoKeepDB
{
    internal class Program
    {
        static void Main()
        {
            Database.Connect();
            WriteLine("Database Successfully Connected!");
            Database.CreateAllTables();
            WriteLine("Tables Successfully Created!");

            List<string> userMenu = ["Register", "Login", "Logout", "Change Password", "Change MPIN", "Change Email", "Change Display Name", "Change Cat Theme", "View User Info", "Delete Account"];
            List<string> accMenu = ["Add Account", "Search Accounts (Last Updated - Ascending)", "Search Accounts (Last Updated - Descending)", "Search Accounts (Name - Ascending)", "Search Accounts (Name - Descending)", "View Password", "Update Account", "Delete Account"];
            List<string> tagMenu = ["Add Tag", "View Tags", "Update Tag", "Delete Tag"];
            List<string> backupMenu = ["Export", "Import"];
            string Menu(List<string> menuList, int num = 1) => string.Join("\n", menuList.Select(menuItem => $"[ {menuList.IndexOf(menuItem) + num} ] - {menuItem}"));
            
            while (true)
            {
                WriteLine();
                Write($"=== USER MENU ===\n{Menu(userMenu)}\n\n=== ACCOUNT MENU (SOON) ===\n{Menu(accMenu, userMenu.Count + 1)}\n\n=== TAG MENU (SOON) ===\n{Menu(tagMenu, (userMenu.Count + accMenu.Count) + 1)}\n\n=== BACKUP MENU (SOON) ===\n{Menu(backupMenu, (userMenu.Count + accMenu.Count + tagMenu.Count) + 1)}\n\n[ 0 ] - Exit\n\nWhat do you want to test? ");

                if (int.TryParse(ReadLine(), out int selected))
                {
                    Clear();
                    switch (selected)
                    {
                        case 0:
                            {
                                Database.Disconnect();
                                return;
                            }
                        case 1:
                            {
                                // ================================== Test User Creation ==================================
                                WriteLine("=== TEST USER CREATION ===");
                                Write("Enter User Display Name: ");
                                string displayName = ReadLine()!;

                                Write("Enter User Email: ");
                                string email = ReadLine()!;
                                if (!Utils.ValidateEmail(email)) break;

                                Write("Enter User Password: ");
                                string password = ReadLine()!;
                                if (!Utils.ValidatePassword(password)) break;
                                string encryptedPassword = Crypto.Encrypt(password);

                                Write("Enter User MPIN: ");
                                string mpin = ReadLine()!;
                                if (!Utils.ValidateMpin(mpin)) break;
                                string encryptedMpin = Crypto.Encrypt(mpin);

                                Write("Enter Cat Theme Preset ID: ");
                                int catThemePresetId = int.Parse(ReadLine()!);

                                Database.CreateUser(displayName, email, encryptedPassword, encryptedMpin, catThemePresetId);
                                WriteLine($"User \"{displayName}\" Successfully Created!");
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

                                int user = Database.AuthenticateUser(email, password);
                                if (User.Session == null) break;
                                else if (!Utils.ValidateEmail(email)) break;
                                else if (!Utils.ValidatePassword(password)) break;
                                else if (user > 0)
                                {
                                    WriteLine($"\n=== USER DEAILS ===\nID: {User.Session.Id}\nDisplay Name: {User.Session.DisplayName}\nEmail: {User.Session.Email}\nCat Preset Id: {User.Session.CatPresetId}");

                                    Write("\n\nTest MPIN? (y, n): ");
                                    if (string.Equals(ReadLine()!.ToLower(), "y"))
                                    {
                                        Write("Enter User MPIN: ");
                                        string mpin = ReadLine()!;
                                        bool mpinValid = User.VerifyMpin(mpin);
                                        WriteLine(mpinValid ? "MPIN is correct!" : "MPIN is incorrect!");
                                    }
                                }
                                else if (user == 0) WriteLine("Incorrect password.");
                                else WriteLine("User not found.");
                                break;
                                // =================================== Test User Login ===================================
                            }
                        case 3:
                            {
                                // =================================== Test User Logout ===================================
                                User.Logout();
                                WriteLine("User Logout Successfully!");
                                break;
                                // =================================== Test User Logout ===================================
                            }
                        case 4:
                            {
                                // ================================= Test Change Password =================================
                                WriteLine("=== TEST CHANGE PASSWORD ===");
                                Write("Enter Old Password: ");
                                string oldPassword = ReadLine()!;
                                Write("Enter New Password: ");
                                string newPassword = ReadLine()!;
                                Write("Re-Enter New Password: ");
                                string newPassword2 = ReadLine()!;

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else if (!User.VerifyPassword(oldPassword)) WriteLine("Wrong old password!");
                                else if (!newPassword.Equals(newPassword2)) WriteLine("New password do not match!");
                                else if (User.VerifyPassword(newPassword)) WriteLine("Your new password must be different from the old password.");
                                else if (!Utils.ValidatePassword(newPassword)) break;
                                else
                                {
                                    string encryptedNewPassword = Crypto.Encrypt(newPassword);
                                    Database.UpdateUserPassword(User.Session.Id, encryptedNewPassword);
                                    WriteLine("Password changed successfully!");
                                }
                                break;
                                // ================================= Test Change Password =================================
                            }
                        case 5:
                            {
                                // =================================== Test Change MPIN ===================================
                                WriteLine("=== TEST CHANGE MPIN ===");
                                Write("Enter Old MPIN: ");
                                string oldMpin = ReadLine()!;
                                Write("Enter New MPIN: ");
                                string newMpin = ReadLine()!;
                                Write("Re-Enter New MPIN: ");
                                string newMpin2 = ReadLine()!;

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else if (!User.VerifyMpin(oldMpin)) WriteLine("Wrong old MPIN!");
                                else if (!newMpin.Equals(newMpin2)) WriteLine("New MPIN do not match!");
                                else if (User.VerifyMpin(newMpin)) WriteLine("Your new MPIN must be different from the old MPIN.");
                                else if (!Utils.ValidateMpin(newMpin)) break;
                                else
                                {
                                    string encryptedNewMpin = Crypto.Encrypt(newMpin);
                                    Database.UpdateUserMpin(User.Session.Id, encryptedNewMpin);
                                    WriteLine("MPIN changed successfully!");
                                }
                                break;
                                // =================================== Test Change MPIN ===================================
                            }
                        case 6:
                            {
                                // =================================== Test Change Email ===================================
                                WriteLine("=== TEST CHANGE EMAIL ===");
                                Write("Enter Old Email: ");
                                string oldEmail = ReadLine()!;
                                Write("Enter New Email: ");
                                string newEmail = ReadLine()!;
                                Write("Re-Enter New Email: ");
                                string newEmail2 = ReadLine()!;

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else if (!User.Session.Email.Equals(oldEmail)) WriteLine("Wrong old Email!");
                                else if (!newEmail.Equals(newEmail2)) WriteLine("New Email do not match!");
                                else if (User.Session.Email.Equals(newEmail)) WriteLine("Your new Email must be different from the old Email.");
                                else if (!Utils.ValidateEmail(newEmail)) break;
                                else
                                {
                                    Database.UpdateUserEmail(User.Session.Id, newEmail);
                                    WriteLine("Email changed successfully!");
                                }
                                break;
                                // =================================== Test Change Email ===================================
                            }
                        case 7:
                            {
                                // ================================ Test Change Display Name ================================
                                WriteLine("=== TEST CHANGE DISPLAY NAME ===");
                                Write("Enter New Display Name: ");
                                string newDisplayName = ReadLine()!;

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else if (User.Session.DisplayName.Equals(newDisplayName)) WriteLine("Your new Display Name must be different from the old Display Name.");
                                else
                                {
                                    Database.UpdateUserDisplayName(User.Session.Id, newDisplayName);
                                    WriteLine("Display Name changed successfully!");
                                }
                                break;
                                // ================================ Test Change Display Name ================================
                            }
                        case 8:
                            {
                                // ================================ Test Change Cat Preset Id ================================
                                WriteLine("=== TEST CHANGE CAT PRESET ID ===");
                                Write("Enter New Cat Preset Id: ");
                                int newCatPresetId = int.Parse(ReadLine()!);

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else if (User.Session.CatPresetId == newCatPresetId) WriteLine("Your new Cat Preset Id must be different from the old Cat Preset Id.");
                                else
                                {
                                    Database.UpdateUserCatPresetId(User.Session.Id, newCatPresetId);
                                    WriteLine("Cat Preset Id changed successfully!");
                                }
                                break;
                                // ================================ Test Change Cat Preset Id ================================
                            }
                        case 9:
                            {
                                // ================================ Test View User ================================
                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else WriteLine($"\n=== USER DEAILS ===\nID: {User.Session.Id}\nDisplay Name: {User.Session.DisplayName}\nEmail: {User.Session.Email}\nCat Preset Id: {User.Session.CatPresetId}");
                                break;
                                // ================================ Test View User ================================
                            }
                        case 10:
                            {
                                // ================================ Test User Deletion ================================
                                WriteLine("=== TEST CHANGE USER DELETE ===");

                                if (User.Session == null) WriteLine("User is not authenticated.");
                                else
                                {
                                    Write("Are you sure you want to delete this account? (y, n): ");
                                    string ans = ReadLine()!;

                                    if (ans.Equals("y"))
                                    {
                                        Database.DeleteUser(User.Session.Id);
                                        WriteLine("User deleted successfully!");
                                    }
                                }
                                break;
                                // ================================ Test User Deletion ================================
                            }
                        default:
                            {
                                WriteLine("Invalid/Unavailable Selection!");
                                break;
                            }
                    }
                }
                else WriteLine("Invalid Selection!");

                ReadKey(true);
                Clear();
            }
        }
    }
}
