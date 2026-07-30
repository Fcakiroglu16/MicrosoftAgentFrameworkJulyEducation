using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.McpServerWithStdio.Resources;

[McpServerResourceType]
internal class UserProfileResources
{
  
    private record UserProfile(string Id, string Name, string Email, string Role, string JoinedAt);

    private static readonly Dictionary<string, UserProfile> Users = new()
    {
        ["1"] = new("1", "Alice Johnson", "alice@example.com", "admin",  "2024-01-15"),
        ["2"] = new("2", "Bob Smith",     "bob@example.com",   "editor", "2024-03-22"),
        ["3"] = new("3", "Carol White",   "carol@example.com", "viewer", "2025-07-10"),
    };

    
    [McpServerResource(UriTemplate = "users://list", Name = "User List", MimeType = "application/json")]
    [Description("Returns a summary list of all registered users.")]
    public static string GetUserList() =>
        JsonSerializer.Serialize(
            Users.Values.Select(u => new { u.Id, u.Name, u.Role }),
            new JsonSerializerOptions { WriteIndented = true });



    [McpServerResource(UriTemplate = "users://profiles/{id}", Name = "User Profile")]
    [Description("Returns the full profile of a user by ID (try:1,2,3).")]
    public static TextResourceContents GetProfile(string id)
    {
        if (!Users.TryGetValue(id, out var user))
            throw new McpException($"User not found: '{id}'. Available IDs: {string.Join(", ", Users.Keys)}");

        return new TextResourceContents
        {
            Uri = $"users://profiles/{id}",
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true })
        };
    }

    
    [McpServerResource(UriTemplate = "users://roles/{role}/members", Name = "Users by Role")]
    [Description("Returns all users that have the specified role (try: admin, editor, viewer).")]
    public static TextResourceContents GetByRole(string role)
    {
        var members = Users.Values.Where(u => u.Role == role).ToList();

        if (members.Count == 0)
            throw new McpException($"No users with role '{role}'. Available roles: admin, editor, viewer.");

        return new TextResourceContents
        {
            Uri = $"users://roles/{role}/members",
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(members, new JsonSerializerOptions { WriteIndented = true })
        };
    }
}