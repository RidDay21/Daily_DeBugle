using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int userId);
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(User user, string password);
        Task<bool> UserExistsAsync(string username);
    }
}