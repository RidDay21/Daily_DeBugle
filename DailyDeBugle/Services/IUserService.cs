using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
    }
}