using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public abstract class Account(IAccount account)
    {
        protected readonly int id = account.Id;
        protected readonly int userId = account.UserId;
        protected readonly string displayName = account.DisplayName;
        protected readonly string email = account.Email;
        protected readonly string? note = account.Note;
        protected readonly List<ITag> tags = account.Tags;
        protected readonly DateTime? updatedAt = account.UpdatedAt;

        public abstract IAccount ViewAccount();
        public abstract void UpdateAccount(IAccount account);

    }

    public class OAuthAccount(IOAuthAccount oAuthAccount) : Account(oAuthAccount)
    {
        private readonly string provider = oAuthAccount.Provider;

        public override IOAuthAccount ViewAccount() => new OAuthAccountDto
        {
            Id = id,
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            Provider = provider,
            Note = note,
            Tags = tags,
            UpdatedAt = updatedAt
        };

        public override void UpdateAccount(IOAuthAccount account)
        {
            IOAuthAccount updatedAccount = new OAuthAccountDto
            {
                Id = id,
                UserId = userId,
                DisplayName = account.DisplayName,
                Email = account.Email,
                Provider = account.Provider,
                Note = account.Note,
                Tags = account.Tags,
            };
        }
    }

    public class CustomAccount(ICustomAccount customAccount) : Account(customAccount)
    {
        private readonly string password = customAccount.Password!;
        public override ICustomAccount ViewAccount() => new CustomAccountDto
        {
            Id = id,
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            Note = note,
            Tags = tags,
            UpdatedAt = updatedAt
        };

        public string ViewPassword(string mpin)
        {
            if (User.VerifyMpin(mpin))
            {
                return password;
            }
            else
            {
                Utils.ThrowError("Invalid MPIN provided!");
                return string.Empty;
            }
        }
    }
}
