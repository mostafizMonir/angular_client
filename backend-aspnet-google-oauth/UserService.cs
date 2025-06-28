using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using GoogleLoginApi.Models;

namespace GoogleLoginApi.Services
{
    public interface IUserService
    {
        Task<UserInfo?> GetUserByEmailAsync(string email);
        Task<UserInfo> CreateOrUpdateUserAsync(GoogleUserInfo googleUser);
        Task<UserInfo?> ValidateUserAsync(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserInfo?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null) return null;

                return new UserInfo
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Name = user.Name,
                    Picture = user.Picture,
                    Provider = user.Provider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return null;
            }
        }

        public async Task<UserInfo> CreateOrUpdateUserAsync(GoogleUserInfo googleUser)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == googleUser.Email);

                if (existingUser != null)
                {
                    // Update existing user
                    existingUser.Name = googleUser.Name;
                    existingUser.Picture = googleUser.Picture;
                    existingUser.LastLoginAt = DateTime.UtcNow;
                    existingUser.Provider = "google";

                    await _context.SaveChangesAsync();

                    return new UserInfo
                    {
                        Id = existingUser.Id.ToString(),
                        Email = existingUser.Email,
                        Name = existingUser.Name,
                        Picture = existingUser.Picture,
                        Provider = existingUser.Provider
                    };
                }
                else
                {
                    // Create new user
                    var newUser = new User
                    {
                        Email = googleUser.Email,
                        Name = googleUser.Name,
                        Picture = googleUser.Picture,
                        Provider = "google",
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    return new UserInfo
                    {
                        Id = newUser.Id.ToString(),
                        Email = newUser.Email,
                        Name = newUser.Name,
                        Picture = newUser.Picture,
                        Provider = newUser.Provider
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating user: {Email}", googleUser.Email);
                throw;
            }
        }

        public async Task<UserInfo?> ValidateUserAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == username && u.Provider == "local");

                if (user == null) return null;

                // Verify password hash
                if (!VerifyPassword(password, user.PasswordHash))
                    return null;

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new UserInfo
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Name = user.Name,
                    Picture = user.Picture,
                    Provider = user.Provider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user: {Username}", username);
                return null;
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // TODO: Implement proper password verification
            // This is a placeholder - you should use proper password hashing like BCrypt
            return password == passwordHash; // Replace with proper verification
        }

        public string HashPassword(string password)
        {
            // TODO: Implement proper password hashing
            // This is a placeholder - you should use proper password hashing like BCrypt
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    // Database Models
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          foreach (var entity in modelBuilder.Model.GetEntityTypes())
          {
            // Optional: Set table name to lowercase/snake_case
            entity.SetTableName(entity.GetTableName().ToLower());

            foreach (var property in entity.GetProperties())
            {
              property.SetColumnName(property.Name.ToLower());
            }
          }
      modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // Explicitly set table name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Picture).HasMaxLength(500);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                
                // PostgreSQL specific configurations
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastLoginAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
} 
