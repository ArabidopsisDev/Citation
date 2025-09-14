﻿using System.Text.Json.Serialization;

namespace Citation.Utils.Api
{
    public partial class DeepSeekApi : IDisposable
    {
        /// <summary>
        /// Represents a chat completion request
        /// </summary>
        public class ChatCompletionRequest
        {
            public string Model { get; set; } = "deepseek-chat";
            public List<Message> Messages { get; set; }
            public double? Temperature { get; set; } = 0.7;
            public int? MaxTokens { get; set; }
            public double? TopP { get; set; } = 1.0;
            public double? FrequencyPenalty { get; set; } = 0.0;
            public double? PresencePenalty { get; set; } = 0.0;
            public bool? Stream { get; set; } = false;
            public List<string> Stop { get; set; }
        }

        public class ChatCompletion
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("_object")]
            public string? Object { get; set; }

            [JsonPropertyName("created")]
            public int Created { get; set; }

            [JsonPropertyName("model")]
            public string? Model { get; set; }

            [JsonPropertyName("choices")]
            public Choice[]? Choices { get; set; }

            [JsonPropertyName("usage")]
            public Usage? Usage { get; set; }
        }

        public class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        public class Choice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        public class Message
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}