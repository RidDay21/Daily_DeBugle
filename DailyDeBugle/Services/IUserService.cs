using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IUserService
    {
        System.Threading.Tasks.Task<System.Collections.Generic.List<User>> GetAllAsync();
    }
}