using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.OrderBy(u => u.Username).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed) return null;

            return user;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task UpdateUserRoleAsync(int userId, UserRole newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await _context.SaveChangesAsync();
            }
        }
    }
}
