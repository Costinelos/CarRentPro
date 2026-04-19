using CarRentPro.Models;

namespace CarRentPro.Interfaces
{
    public interface IBlacklistService
    {
        Task<bool> CheckIfUserCanRentAsync(string userId);
        Task AddToBlacklistAsync(string userId, string reason, string adminId, DateTime? expirationDate = null);
        Task RemoveFromBlacklistAsync(int blacklistEntryId, string adminId);
        Task<List<BlacklistEntry>> GetBlacklistAsync();
        Task<BlacklistEntry> GetBlacklistEntryAsync(int id);
    }
}