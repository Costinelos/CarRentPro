using CarRentPro;
using CarRentPro.Interfaces;
using CarRentPro.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentPro.Repositories
{
    public class BlacklistRepository : IBlacklistRepository
    {
        private readonly ApplicationDbContext _context;

        public BlacklistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserBlacklistedAsync(string userId)
        {
            return await _context.BlacklistEntries
                .AnyAsync(b => b.UserId == userId && b.IsActive &&
                              (b.ExpirationDate == null || b.ExpirationDate > DateTime.UtcNow));
        }

        public async Task AddToBlacklistAsync(string userId, string reason, string adminId, DateTime? expirationDate = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var entry = new BlacklistEntry
            {
                UserId = userId,
                Reason = reason,
                CreatedByAdminId = adminId,
                ExpirationDate = expirationDate,
                IsActive = true
            };

            user.IsBlacklisted = true;
            _context.BlacklistEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromBlacklistAsync(int blacklistEntryId)
        {
            var entry = await _context.BlacklistEntries.FindAsync(blacklistEntryId);
            if (entry != null)
            {
                entry.IsActive = false;
                var user = await _context.Users.FindAsync(entry.UserId);
                if (user != null)
                {
                    var hasOtherActiveEntries = await _context.BlacklistEntries
                        .AnyAsync(b => b.UserId == user.Id && b.Id != blacklistEntryId && b.IsActive);

                    if (!hasOtherActiveEntries)
                    {
                        user.IsBlacklisted = false;
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BlacklistEntry>> GetAllBlacklistedUsersAsync()
        {
            return await _context.BlacklistEntries
                .Include(b => b.User)
                .Where(b => b.IsActive)
                .ToListAsync();
        }

        public async Task<BlacklistEntry> GetBlacklistEntryAsync(int id)
        {
            return await _context.BlacklistEntries
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}