using CoreCare.Models;

namespace CoreCare.Services
{
    public static class SessionService
    {
        public static User? CurrentUser { get; private set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static void SignIn(User user)
        {
            CurrentUser = user;
        }

        public static void SignOut()
        {
            CurrentUser = null;
        }
    }
}