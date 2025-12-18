using Dapper;
using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.Entities;

namespace Bibliotheque.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly DbContext _context;

    public UserRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT u.id, u.email, u.password_hash AS PasswordHash, u.first_name AS FirstName, 
                   u.last_name AS LastName, u.role_id AS RoleId, u.created_at AS CreatedAt,
                   r.name AS RoleName, r.access_level AS AccessLevel
            FROM users u
            JOIN roles r ON u.role_id = r.id
            WHERE u.email = @Email";
        
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT u.id, u.email, u.password_hash AS PasswordHash, u.first_name AS FirstName, 
                   u.last_name AS LastName, u.role_id AS RoleId, u.created_at AS CreatedAt,
                   r.name AS RoleName, r.access_level AS AccessLevel
            FROM users u
            JOIN roles r ON u.role_id = r.id
            WHERE u.id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO users (email, password_hash, first_name, last_name, role_id)
            VALUES (@Email, @PasswordHash, @FirstName, @LastName, @RoleId);
            SELECT LAST_INSERT_ID();";
        
        user.Id = await connection.ExecuteScalarAsync<int>(sql, user);
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM users WHERE email = @Email";
        return await connection.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
    }
}
