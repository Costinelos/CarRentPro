using CarRentPro.Interfaces;
using CarRentPro.Models;
using Microsoft.Extensions.Logging;

namespace CarRentPro.Services
{
    public class BlacklistService : IBlacklistService
    {
        private readonly IBlacklistRepository _repository;
        private readonly ILogger<BlacklistService> _logger;

        public BlacklistService(IBlacklistRepository repository, ILogger<BlacklistService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> CheckIfUserCanRentAsync(string userId)
        {
            try
            {
                return !await _repository.IsUserBlacklistedAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blacklist for user {UserId}", userId);
                return false;
            }
        }

        public async Task AddToBlacklistAsync(string userId, string reason, string adminId, DateTime? expirationDate = null)
        {
            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException("Reason is required");

            await _repository.AddToBlacklistAsync(userId, reason, adminId, expirationDate);
            _logger.LogInformation("User {UserId} blacklisted by admin {AdminId}. Reason: {Reason}",
                userId, adminId, reason);
        }

        public async Task RemoveFromBlacklistAsync(int blacklistEntryId, string adminId)
        {
            await _repository.RemoveFromBlacklistAsync(blacklistEntryId);
            _logger.LogInformation("Blacklist entry {EntryId} removed by admin {AdminId}",
                blacklistEntryId, adminId);
        }

        public async Task<List<BlacklistEntry>> GetBlacklistAsync()
        {
            return await _repository.GetAllBlacklistedUsersAsync();
        }

        public async Task<BlacklistEntry> GetBlacklistEntryAsync(int id)
        {
            return await _repository.GetBlacklistEntryAsync(id);
        }
    }
}