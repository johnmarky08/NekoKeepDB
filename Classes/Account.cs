using NekoKeepDB.Databases;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public abstract class Account
    {
        public abstract Account ViewAccount();
    }

    public class OAuthAccount : Account
    {
        public override OAuthAccount ViewAccount()
        {
            throw new NotImplementedException();
        }
    }

    public class CustomAccount : Account
    {
        public override CustomAccount ViewAccount()
        {
            throw new NotImplementedException();
        }
    }
}
