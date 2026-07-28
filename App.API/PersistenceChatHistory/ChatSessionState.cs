using System.ComponentModel.DataAnnotations;

namespace App.API.PersistenceChatHistory;

public class ChatSessionState
{
    [Key] public string ConversationId { get; set; } = string.Empty;

    public string MessagesJson { get; set; } = "[]";
}