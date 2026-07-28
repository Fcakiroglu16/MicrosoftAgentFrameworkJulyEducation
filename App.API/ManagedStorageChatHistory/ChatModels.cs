namespace App.API.ManagedStorageChatHistory;

public sealed record ChatRequest(string Message, string? ConversationId = null);

public sealed record ChatResponse(string ConversationId, string Reply);
