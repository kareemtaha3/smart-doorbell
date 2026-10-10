using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var input = username.Trim().ToLower();
        return await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == input, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var input = email.Trim().ToLower();
        return await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var input = username.Trim().ToLower();
        return await _db.Users.AnyAsync(u => u.Username.ToLower() == input, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var input = email.Trim().ToLower();
        return await _db.Users.AnyAsync(u => u.Email.ToLower() == input, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
    }
}
