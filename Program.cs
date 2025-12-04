using NekoKeepDB.Classes;
using NekoKeepDB.Databases;
using NekoKeepDB.Interfaces;
using static System.Console;

namespace NekoKeepDB
{
    internal class Program
    {
        static void Main()
        {
            DotNetEnv.Env.Load();
            MainDB.Connect();
            WriteLine("Database Successfully Connected!");
            // MainDB.DropAllTables(); // Uncomment to drop tables 
            MainDB.CreateAllTables();
            WriteLine("Tables Successfully Created!");

            List<string> userMenu = ["Register", "Login", "Logout", "Change Password", "Change MPIN", "Change Email", "Change Display Name", "Change Cat Theme", "View User Info", "Delete Account"];
            List<string> accMenu = ["Add Account", "Filter Accounts (Last Updated - Ascending)", "Filter Accounts (Last Updated - Descending)", "Filter Accounts (Name - Ascending)", "Filter Accounts (Name - Descending)", "View Password", "Update Account", "Delete Account", "Search an Account"];
            List<string> tagMenu = ["Add Tag", "View Tags", "Update Tag", "Delete Tag"];
            List<string> backupMenu = ["Export", "Import"];
            string Menu(List<string> menuList, int num = 1) => string.Join("\n", menuList.Select(menuItem => $"[ {menuList.IndexOf(menuItem) + num} ] - {menuItem}"));

            while (true)
            {
                try
                {
                    WriteLine();
                    Write($"=== USER MENU ===\n{Menu(userMenu)}\n\n=== ACCOUNT MENU ===\n{Menu(accMenu, userMenu.Count + 1)}\n\n=== TAG MENU ===\n{Menu(tagMenu, (userMenu.Count + accMenu.Count) + 1)}\n\n=== BACKUP MENU ===\n{Menu(backupMenu, (userMenu.Count + accMenu.Count + tagMenu.Count) + 1)}\n\n[ -1 ] - Generate Master Key\n[ 0 ] - Exit\n\nWhat do you want to test? ");

                    if (int.TryParse(ReadLine(), out int selected))
                    {
                        Clear();
                        switch (selected)
                        {
                            case -1:
                                {
                                    WriteLine("Master Key: " + Crypto.GenerateMasterKeyBase64());
                                    return;
                                }
                            case 0:
                                {
                                    MainDB.Disconnect();
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
                                    string encryptedPassword = Utils.BCryptEncrypt(password);

                                    Write("Enter User MPIN: ");
                                    string mpin = ReadLine()!;
                                    if (!Utils.ValidateMpin(mpin)) break;
                                    string encryptedMpin = Utils.BCryptEncrypt(mpin);

                                    Write("Enter Cat Theme Preset ID: ");
                                    int catThemePresetId = int.Parse(ReadLine()!);

                                    IUser user = new UserDto()
                                    {
                                        DisplayName = displayName,
                                        Email = email,
                                        CatPresetId = catThemePresetId,
                                    };
                                    UsersDB.CreateUser(user, encryptedPassword, encryptedMpin);
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

                                    int user = UsersDB.AuthenticateUser(email, password);
                                    if (!Utils.ValidateEmail(email)) break;
                                    else if (!Utils.ValidatePassword(password)) break;
                                    else if (user > 0 && (User.Session != null))
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
                                        UsersDB.UpdateUserPassword(User.Session.Id, newPassword);
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
                                        UsersDB.UpdateUserMpin(User.Session.Id, newMpin);
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
                                        UsersDB.UpdateUserEmail(User.Session.Id, newEmail);
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
                                        UsersDB.UpdateUserDisplayName(User.Session.Id, newDisplayName);
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
                                        UsersDB.UpdateUserCatPresetId(User.Session.Id, newCatPresetId);
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
                                            UsersDB.DeleteUser(User.Session.Id);
                                            WriteLine("User deleted successfully!");
                                        }
                                    }
                                    break;
                                    // ================================ Test User Deletion ================================
                                }
                            case 11:
                                {
                                    // ================================== Test Account Creation ==================================
                                    WriteLine("=== TEST ACCOUNT CREATION ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account Display Name: ");
                                    string displayName = ReadLine()!;

                                    Write("Enter Account Note (Optional): ");
                                    string? note = ReadLine();
                                    note = string.IsNullOrWhiteSpace(note) ? null : note;

                                    Write("Enter Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    Write("Enter Account Email: ");
                                    string email = ReadLine()!;
                                    if (!Utils.ValidateEmail(email)) break;

                                    Write("Enter Account Type: ");
                                    string type = ReadLine()!;
                                    if (!Utils.ValidateType(type)) break;

                                    if (type.Equals("OAuth"))
                                    {
                                        Write("Enter Account Provider: ");
                                        string provider = ReadLine()!;
                                        IOAuthAccount oAuthAccount = new OAuthAccountDto()
                                        {
                                            UserId = User.Session!.Id,
                                            Email = email,
                                            DisplayName = displayName,
                                            Provider = provider,
                                            Tags = tags,
                                            Note = note,
                                        };
                                        AccountsDB.CreateAccount(oAuthAccount);
                                    }
                                    else
                                    {
                                        Write("Enter User Password: ");
                                        string password = ReadLine()!;
                                        ICustomAccount customAccount = new CustomAccountDto()
                                        {
                                            UserId = User.Session!.Id,
                                            Email = email,
                                            DisplayName = displayName,
                                            Password = password,
                                            Tags = tags,
                                            Note = note,
                                        };
                                        AccountsDB.CreateAccount(customAccount);
                                    }

                                    WriteLine($"Account \"{displayName}\" Successfully Created!");
                                    break;
                                    // ================================== Test Account Creation ==================================
                                }
                            case 12:
                                {
                                    // ================================== Test Account Retrieval ==================================
                                    WriteLine("=== TEST ACCOUNT RETRIEVAL ===");
                                    WriteLine("> By Last Updated (Ascending)");
                                    if (!Utils.IsAuthenticated()) break;
                                    
                                    Write("Enter Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    var accounts = User.ViewAccounts(true, false, tags);

                                    WriteLine("\n=== OAUTH ACCOUNTS ===");
                                    foreach (OAuthAccount accountData in accounts.OfType<OAuthAccount>().ToList())
                                    {
                                        IOAuthAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Provider: {account.Provider} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    WriteLine("\n=== CUSTOM ACCOUNTS ===");
                                    foreach (CustomAccount accountData in accounts.OfType<CustomAccount>().ToList())
                                    {
                                        ICustomAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    break;
                                    // ================================== Test Account Retrieval ==================================
                                }
                            case 13:
                                {
                                    // ================================== Test Account Retrieval ==================================
                                    WriteLine("=== TEST ACCOUNT RETRIEVAL ===");
                                    WriteLine("> By Last Updated (Descending)");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    var accounts = User.ViewAccounts(true, true, tags);

                                    WriteLine("\n=== OAUTH ACCOUNTS ===");
                                    foreach (OAuthAccount accountData in accounts.OfType<OAuthAccount>().ToList())
                                    {
                                        IOAuthAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Provider: {account.Provider} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    WriteLine("\n=== CUSTOM ACCOUNTS ===");
                                    foreach (CustomAccount accountData in accounts.OfType<CustomAccount>().ToList())
                                    {
                                        ICustomAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    break;
                                    // ================================== Test Account Retrieval ==================================
                                }
                            case 14:
                                {
                                    // ================================== Test Account Retrieval ==================================
                                    WriteLine("=== TEST ACCOUNT RETRIEVAL ===");
                                    WriteLine("> By Display Name (Ascending)");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    var accounts = User.ViewAccounts(false, false, tags);

                                    WriteLine("\n=== OAUTH ACCOUNTS ===");
                                    foreach (OAuthAccount accountData in accounts.OfType<OAuthAccount>().ToList())
                                    {
                                        IOAuthAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Provider: {account.Provider} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    WriteLine("\n=== CUSTOM ACCOUNTS ===");
                                    foreach (CustomAccount accountData in accounts.OfType<CustomAccount>().ToList())
                                    {
                                        ICustomAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    break;
                                    // ================================== Test Account Retrieval ==================================
                                }
                            case 15:
                                {
                                    // ================================== Test Account Retrieval ==================================
                                    WriteLine("=== TEST ACCOUNT RETRIEVAL ===");
                                    WriteLine("> By Display Name (Descending)");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    var accounts = User.ViewAccounts(false, true, tags);

                                    WriteLine("\n=== OAUTH ACCOUNTS ===");
                                    foreach (OAuthAccount accountData in accounts.OfType<OAuthAccount>().ToList())
                                    {
                                        IOAuthAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Provider: {account.Provider} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    WriteLine("\n=== CUSTOM ACCOUNTS ===");
                                    foreach (CustomAccount accountData in accounts.OfType<CustomAccount>().ToList())
                                    {
                                        ICustomAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    break;
                                    // ================================== Test Account Retrieval ==================================
                                }
                            case 16:
                                {
                                    // ================================== Test Account Password Retrieval ==================================
                                    WriteLine("=== TEST ACCOUNT PASSWORD RETRIEVAL ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    List<Account> accounts = User.Session!.Accounts!;

                                    Write("Enter account id: ");
                                    int id = int.Parse(ReadLine()!);

                                    Account? accountData = accounts.FirstOrDefault(a => a.Data.Id == id);
                                    if (accountData == null) WriteLine("No account found with the said id.");
                                    else if (accountData.GetType() != typeof(CustomAccount)) WriteLine("Account is not a custom account!");
                                    else
                                    {
                                        CustomAccount customAccount = (CustomAccount)accountData;
                                        Write("Enter MPIN: ");
                                        string mpin = ReadLine()!;
                                        string password = customAccount.ViewPassword(mpin);

                                        if (!string.IsNullOrWhiteSpace(password)) WriteLine("Password: " + password);
                                    }

                                    break;
                                    // ================================== Test Account Password Retrieval ==================================
                                }
                            case 17:
                                {
                                    // ================================== Test Account Update ==================================
                                    WriteLine("=== TEST ACCOUNT UPDATE ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account ID: ");
                                    int accountId = int.Parse(ReadLine()!);

                                    Account? accountData = User.Session!.Accounts!.FirstOrDefault(a => a.Data.Id == accountId);
                                    if (accountData == null)
                                    {
                                        WriteLine("Invalid Account ID!");
                                        break;
                                    }

                                    string accountType = accountData.GetType() == typeof(OAuthAccount) ? "OAuth" : "Custom";

                                    Write("Enter New Account Display Name: ");
                                    string displayName = ReadLine()!;

                                    Write("Enter New Account Note (Optional): ");
                                    string? note = ReadLine();
                                    note = string.IsNullOrWhiteSpace(note) ? null : note;

                                    Write("Enter New Account Tag IDs (comma-separated): ");
                                    string? tagsInput = ReadLine();
                                    List<ITag> tags = !string.IsNullOrWhiteSpace(tagsInput) ? [.. tagsInput.Split(",").Select(tag => (ITag)(new TagDto() { Id = int.Parse(tag) }))] : [];

                                    Write("Enter New Account Email: ");
                                    string email = ReadLine()!;
                                    if (!Utils.ValidateEmail(email)) break;

                                    if (accountType.Equals("OAuth"))
                                    {
                                        Write("Enter New Account Provider: ");
                                        string provider = ReadLine()!;
                                        IOAuthAccount oAuthAccount = new OAuthAccountDto()
                                        {
                                            Id = accountId,
                                            UserId = User.Session!.Id,
                                            Email = email,
                                            DisplayName = displayName,
                                            Provider = provider,
                                            Tags = tags,
                                            Note = note,
                                        };
                                        AccountsDB.UpdateAccount(oAuthAccount);
                                    }
                                    else
                                    {
                                        Write("Enter New User Password: ");
                                        string password = ReadLine()!;
                                        ICustomAccount customAccount = new CustomAccountDto()
                                        {
                                            Id = accountId,
                                            UserId = User.Session!.Id,
                                            Email = email,
                                            DisplayName = displayName,
                                            Password = password,
                                            Tags = tags,
                                            Note = note,
                                        };
                                        AccountsDB.UpdateAccount(customAccount);
                                    }

                                    WriteLine($"Account \"{displayName}\" Successfully Edited!");
                                    break;
                                    // ================================== Test Account Update ==================================
                                }
                            case 18:
                                {
                                    // ================================== Test Account Deletion ==================================
                                    WriteLine("=== TEST ACCOUNT DELETION ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Account ID: ");
                                    int accountId = int.Parse(ReadLine()!);
                                    Account? accountData = User.Session!.Accounts!.FirstOrDefault(a => a.Data.Id == accountId);
                                    if (accountData == null)
                                    {
                                        WriteLine("Invalid Account ID!");
                                        break;
                                    }

                                    AccountsDB.DeleteAccount(accountId);
                                    WriteLine("Account Successfully Deleted!");
                                    break;
                                    // ================================== Test Account Deletion ==================================
                                }
                            case 19:
                                {
                                    // ================================== Test Search Account ==================================
                                    WriteLine("=== TEST SEARCH ACCOUNT ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Search: ");
                                    string search = ReadLine()!;

                                    List<Account> accounts = Search.Get(search);

                                    WriteLine("\n=== OAUTH ACCOUNTS ===");
                                    foreach (OAuthAccount accountData in accounts.OfType<OAuthAccount>().ToList())
                                    {
                                        IOAuthAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Provider: {account.Provider} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    WriteLine("\n=== CUSTOM ACCOUNTS ===");
                                    foreach (CustomAccount accountData in accounts.OfType<CustomAccount>().ToList())
                                    {
                                        ICustomAccount account = accountData.Data;
                                        WriteLine($"[ {account.Id} ] - User Id: {account.UserId} - Display Name: {account.DisplayName} - Email: {account.Email} - Tags: {(account.Tags.Count != 0 ? string.Join(", ", account.Tags.Select(tag => tag.DisplayName)) : "No Tag")} - Note: {(account.Note ?? "No Note")}");
                                    }

                                    break;
                                    // ================================== Test Search Account ==================================
                                }
                            case 20:
                                {
                                    // ================================== Test Tag Creation ==================================
                                    WriteLine("=== TEST TAG CREATION ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Tag Display Name: ");
                                    string displayName = ReadLine()!;

                                    TagsDB.CreateTag(User.Session!.Id, displayName);

                                    WriteLine($"Tag \"{displayName}\" Successfully Created!");
                                    break;
                                    // ================================== Test Tag Creation ==================================
                                }
                            case 21:
                                {
                                    // ================================== Test Tag Retrieval ==================================
                                    WriteLine("=== TEST TAG RETRIEVAL ===");
                                    if (!Utils.IsAuthenticated()) break;
                                    List<ITag> tags = TagsDB.RetrieveTags(User.Session!.Id);

                                    foreach (ITag tag in tags) WriteLine($"[ {tag.Id} ] - {tag.DisplayName}");

                                    break;
                                    // ================================== Test Tag Retrieval ==================================
                                }
                            case 22:
                                {
                                    // ================================== Test Modify Tag ==================================
                                    WriteLine("=== TEST TAG MODIFICATION ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Tag ID: ");
                                    int tagId = int.Parse(ReadLine()!);
                                    Write("Enter New Tag Display Name: ");
                                    string displayName = ReadLine()!;

                                    if (TagsDB.RetrieveTag(tagId) == null)
                                    {
                                        WriteLine("Invalid Tag ID!");
                                        break;
                                    }

                                    ITag tag = new TagDto()
                                    {
                                        Id = tagId,
                                        DisplayName = displayName,
                                    };
                                    TagsDB.UpdateTag(tag);

                                    WriteLine($"Tag \"{displayName}\" Successfully Edited!");
                                    break;
                                    // ================================== Test Modify Tag ==================================
                                }
                            case 23:
                                {
                                    // ================================== Test Tag Deletion ==================================
                                    WriteLine("=== TEST TAG DELETION ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter Tag ID: ");
                                    int tagId = int.Parse(ReadLine()!);
                                    if (TagsDB.RetrieveTag(tagId) == null)
                                    {
                                        WriteLine("Invalid Tag ID!");
                                        break;
                                    }

                                    TagsDB.DeleteTag(tagId);
                                    WriteLine("Tag Successfully Deleted!");
                                    break;
                                    // ================================== Test Tag Deletion ==================================
                                }
                            case 24:
                                {
                                    // ================================== Test Account Export ==================================
                                    WriteLine("=== TEST ACCOUNT EXPORT ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter File Path (e.g., C:\\Users\\Marky\\OneDrive\\Desktop\\neko_keep_accounts.xlsx): ");
                                    string filePath = ReadLine()!;

                                    Write("Sort by Last Updated? (y/n): ");
                                    string sortByDateInput = ReadLine()!;
                                    bool sortByDate = sortByDateInput.Equals("y");
                                    
                                    Write("Descending Order? (y/n): ");
                                    string descendingInput = ReadLine()!;
                                    bool descending = descendingInput.Equals("y");

                                    Write("Enter MPIN to proceed: ");
                                    string mpin = ReadLine()!;
                                    if (!User.VerifyMpin(mpin))
                                    {
                                        WriteLine("Incorrect MPIN provided!");
                                        break;
                                    }

                                    Backup.Export(filePath, sortByDate, descending, mpin);
                                    WriteLine("Accounts exported successfully!");
                                    break;
                                    // ================================== Test Account Export ==================================
                                }
                            case 25:
                                {
                                    // ================================== Test Account Import ==================================
                                    WriteLine("=== TEST ACCOUNT IMPORT ===");
                                    if (!Utils.IsAuthenticated()) break;

                                    Write("Enter File Path (e.g., C:\\Users\\Marky\\OneDrive\\Desktop\\neko_keep_accounts.xlsx): ");
                                    string filePath = ReadLine()!;

                                    Backup.Import(filePath);
                                    WriteLine("Accounts imported successfully!");
                                    break;
                                    // ================================== Test Account Import ==================================
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
                catch (Exception ex)
                {
                    WriteLine("Invalid Action: " + ex.Message);
                }
            }
        }
    }
}
