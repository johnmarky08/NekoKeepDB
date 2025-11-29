using NekoKeepDB.Databases;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public abstract class Account
    {
    }

    public class OAuthAccount : Account
    {
        public OAuthAccount(IOAuthAccount oAuthAccount)
        {
            AccountsDB.CreateAccount(oAuthAccount);
        }
    }

    public class CustomAccount : Account
    {
        public CustomAccount(ICustomAccount customAccount)
        {
            AccountsDB.CreateAccount(customAccount);
        }
    }
}
