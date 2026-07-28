namespace App.API.InMemoryChatHistory;

public sealed record ChatRequest(string Message, string? ConversationId = null);

public sealed record ChatResponse(string ConversationId, string Reply);

public sealed record ChatMessageDto(string Role, string Text);
