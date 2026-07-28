using Microsoft.EntityFrameworkCore;

namespace App.API.PersistenceChatHistory;

public class ChatHistoryDbContext(DbContextOptions<ChatHistoryDbContext> options) : DbContext(options)
{
    public DbSet<ChatSessionState> ChatSessionStates { get; set; } = null!;
}