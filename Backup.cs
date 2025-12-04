using NekoKeepDB.Classes;
using NekoKeepDB.Databases;
using NekoKeepDB.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace NekoKeepDB
{
    public class Backup
    {
        public static void Export(string filePath, bool sortByDate, bool descending, string mpin)
        {
            // Get the accounts using your existing method
            var accounts = User.ViewAccounts(sortByDate, descending, []);

            // Set the license context for EPPlus
            ExcelPackage.License.SetNonCommercialPersonal("NekoKeep Project");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("NekoKeep - Accounts");

            // Header row
            worksheet.Cells[1, 1].Value = "Type";
            worksheet.Cells[1, 2].Value = "Display Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Provider";
            worksheet.Cells[1, 5].Value = "Password";
            worksheet.Cells[1, 6].Value = "Note";
            worksheet.Cells[1, 7].Value = "Tags";

            // Bold header
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Data rows
            int row = 2;
            foreach (var account in accounts)
            {
                string accountType = account.GetType() == typeof(OAuthAccount) ? "OAuth" : "Custom";
                worksheet.Cells[row, 1].Value = accountType;
                worksheet.Cells[row, 2].Value = account.Data.DisplayName;
                worksheet.Cells[row, 3].Value = account.Data.Email;
                worksheet.Cells[row, 4].Value = accountType.Equals("OAuth") ? ((OAuthAccount)account).Data.Provider : "";
                worksheet.Cells[row, 5].Value = accountType.Equals("Custom") ? ((CustomAccount)account).ViewPassword(mpin) : "";
                worksheet.Cells[row, 6].Value = account.Data.Note;
                worksheet.Cells[row, 7].Value = string.Join(", ", account.Data.Tags.Select(tag => tag.DisplayName));
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Save to file
            package.SaveAs(new FileInfo(filePath));
        }

        public static void Import(string filePath)
        {
            // Set the license context for EPPlus
            ExcelPackage.License.SetNonCommercialPersonal("NekoKeep Project");
            
            var file = new FileInfo(filePath);
            if (!file.Exists) return;

            using var package = new ExcelPackage(file);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return;

            int startRow = 2; // header is row 1
            int lastRow = worksheet.Dimension?.End.Row ?? 0;
            if (lastRow < startRow) return;

            for (int r = startRow; r <= lastRow; r++)
            {
                // Read cells (trim and treat nulls)
                string type = (worksheet.Cells[r, 1].GetValue<string>() ?? "").Trim();
                string displayName = (worksheet.Cells[r, 2].GetValue<string>() ?? "").Trim();
                string email = (worksheet.Cells[r, 3].GetValue<string>() ?? "").Trim();
                string provider = (worksheet.Cells[r, 4].GetValue<string>() ?? "").Trim();
                string passwordPlain = (worksheet.Cells[r, 5].GetValue<string>() ?? "").Trim();
                string tagsCsv = (worksheet.Cells[r, 7].GetValue<string>() ?? "").Trim();
                string? note = (worksheet.Cells[r, 6].GetValue<string>() ?? "").Trim();
                note = string.IsNullOrWhiteSpace(note) ? null : note;

                // Skip rows that are essentially empty
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(email) || !(new List<string>() { "OAuth", "Custom" }).Contains(type) || !Utils.ValidateEmail(email)) continue;
                else if (type.Equals("OAuth") && string.IsNullOrWhiteSpace(provider)) continue;
                else if (type.Equals("Custom") && string.IsNullOrWhiteSpace(passwordPlain) && !Utils.ValidatePassword(passwordPlain)) continue;

                // Parse tags into List<ITag>
                var tagNames = string.IsNullOrWhiteSpace(tagsCsv)
                ? []
                : tagsCsv.Split(", ", StringSplitOptions.RemoveEmptyEntries)
                         .Select(t => t.Trim())
                         .Where(t => t.Length > 0)
                         .ToList();

                List<ITag> tags = ResolveTags(tagNames);

                try
                {
                    if (type.Equals("OAuth"))
                    {
                        // Build OAuth account DTO and create
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
                        // Build Custom account DTO and create (pass plaintext password; CreateAccount will encrypt)
                        ICustomAccount customAccount = new CustomAccountDto()
                        {
                            UserId = User.Session!.Id,
                            Email = email,
                            DisplayName = displayName,
                            Password = passwordPlain,
                            Tags = tags,
                            Note = note,
                        };

                        AccountsDB.CreateAccount(customAccount);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ThrowError($"Failed to import row {r}: {ex.Message}");
                }
            }
        }

        private static List<ITag> ResolveTags(List<string> tagNames)
        {
            int userId = User.Session!.Id;
            var tags = new List<ITag>();
            List<ITag> currentTags = TagsDB.RetrieveTags(userId);

            foreach (var name in tagNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                ITag? existingTag = currentTags.FirstOrDefault(t => t.DisplayName!.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (existingTag != null)
                {
                    tags.Add(existingTag);
                }
                else
                {
                    TagsDB.CreateTag(userId, name);
                    ITag newTag = TagsDB.RetrieveTags(userId).FirstOrDefault(t => t.DisplayName!.Equals(name, StringComparison.OrdinalIgnoreCase))!;
                    tags.Add(newTag);
                }
            }

            return tags;
        }
    }
}
