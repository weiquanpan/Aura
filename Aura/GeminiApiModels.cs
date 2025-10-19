using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aura.Gemini
{
    // Request Models
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Text { get; set; }

        [JsonPropertyName("inline_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InlineData InlineData { get; set; }
    }

    public class InlineData
    {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    // Response Models
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    // The "content" in the response has the same structure as the request's content,
    // so we can reuse the Content and Part classes.
}
