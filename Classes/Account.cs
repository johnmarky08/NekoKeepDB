using NekoKeepDB.Databases;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public abstract class Account(IAccount account)
    {
        protected readonly int id = account.Id;
        protected readonly int userId = account.UserId;
        protected string displayName = account.DisplayName;
        protected string email = account.Email;
        protected string? note = account.Note;
        protected List<ITag> tags = account.Tags;
        protected DateTime? updatedAt = account.UpdatedAt;

        public abstract IAccount Data { get; }
        protected void UpdateAccount(IAccount updatedAccount)
        {
            displayName = updatedAccount.DisplayName;
            email = updatedAccount.Email;
            note = updatedAccount.Note;
            tags = updatedAccount.Tags;
            updatedAt = updatedAccount.UpdatedAt;
        }
    }

    public class OAuthAccount(IOAuthAccount oAuthAccount) : Account(oAuthAccount)
    {
        private string provider = oAuthAccount.Provider;

        public override IOAuthAccount Data => new OAuthAccountDto
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

        public void UpdateAccount(IOAuthAccount updatedOAuthAccount)
        {
            base.UpdateAccount(updatedOAuthAccount);
            tags = TagsDB.RetrieveAccountTags(id);
            updatedAt = DateTime.Now;
            provider = updatedOAuthAccount.Provider;
        }
    }

    public class CustomAccount(ICustomAccount customAccount) : Account(customAccount)
    {
        private string password = customAccount.Password!;
        public override ICustomAccount Data => new CustomAccountDto
        {
            Id = id,
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            Note = note,
            Tags = tags,
            UpdatedAt = updatedAt
        };

        public void UpdateAccount(ICustomAccount updatedCustomAccount)
        {
            base.UpdateAccount(updatedCustomAccount);
            tags = TagsDB.RetrieveAccountTags(id);
            updatedAt = DateTime.Now;
            password = updatedCustomAccount.Password!;
        }

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
