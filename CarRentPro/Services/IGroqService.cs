namespace CarRentPro.Services
{
    public interface IGroqService
    {
        Task<string> GetRecommendationAsync(string userQuery, string vehicleContext);

    }
}