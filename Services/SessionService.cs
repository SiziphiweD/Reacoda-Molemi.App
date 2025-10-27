using ReacodeApp.Models;
using System.Text.Json;

namespace ReacodeApp.Services
{
    public interface ISessionService
    {
        void SetUser(User user);
        User? GetUser();
        void ClearUser();
        bool IsLoggedIn();
        bool IsFarmer();
        bool IsBuyer();
        bool IsAdmin();
        bool IsSuperAdmin();
    }

    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string USER_SESSION_KEY = "CurrentUser";

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetUser(User user)
        {
            var userJson = JsonSerializer.Serialize(user);
            _httpContextAccessor.HttpContext?.Session.SetString(USER_SESSION_KEY, userJson);
        }

        public User? GetUser()
        {
            var userJson = _httpContextAccessor.HttpContext?.Session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<User>(userJson);
        }

        public void ClearUser()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(USER_SESSION_KEY);
        }

        public bool IsLoggedIn()
        {
            return GetUser() != null;
        }

        public bool IsFarmer()
        {
            var user = GetUser();
            return user?.Role == UserRole.Farmer;
        }

        public bool IsBuyer()
        {
            var user = GetUser();
            return user?.Role == UserRole.Buyer;
        }

        public bool IsAdmin()
        {
            var user = GetUser();
            return user?.Role == UserRole.Admin;
        }

        public bool IsSuperAdmin()
        {
            var user = GetUser();
            return user?.Role == UserRole.SuperAdmin;
        }
    }
}
