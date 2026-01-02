using CarRentPro.Models;

namespace CarRentPro.Interfaces
{
    public interface IBlacklistRepository
    {
        Task<bool> IsUserBlacklistedAsync(string userId);
        Task AddToBlacklistAsync(string userId, string reason, string adminId, DateTime? expirationDate = null);
        Task RemoveFromBlacklistAsync(int blacklistEntryId);
        Task<List<BlacklistEntry>> GetAllBlacklistedUsersAsync();
        Task<BlacklistEntry> GetBlacklistEntryAsync(int id);
    }
}