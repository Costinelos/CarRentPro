using System.Text.Json.Serialization;

namespace CarRentPro.Models
{
    public class GroqRequest
    {
        // "llama3-8b-8192" este modelul rapid recomandat de Groq
        [JsonPropertyName("model")]
        public string Model { get; set; } = "llama3-8b-8192";

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } // "user", "system"

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class GroqResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }
}