using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Npgsql;

public class DbService
{
    private readonly string _connectionString;
    
    public DbService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddUserAsync(string username, string email, string password)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT add_user(@username, @email, @password)", connection);
        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("password", password);

        await command.ExecuteNonQueryAsync();
    }

    public async Task SendMessageAsync(int chatId, int senderId, string content)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT send_message(@chatId, @senderId, @content)", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("senderId", senderId);
        command.Parameters.AddWithValue("content", content);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Chat>> GetChatsForUserAsync(int userId)
    {
        var chats = new List<Chat>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("Добавить функцию(@userId)", connection);
        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            chats.Add(new Chat
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsGroup = reader.GetBoolean(2),
                CreatedAt = reader.GetDateTime(3),
            });
        }
        return chats;
    }

    public async Task AddMessageAsync(int chatId, int senderId, string content)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("Добавить функцию отправления сообщений(@chatId, @senderId, @content)", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("senderId", senderId);
        command.Parameters.AddWithValue("content", content);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Message>> GetMessagesForChatAsync(int chatId)
    {
        var messages = new List<Message>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("Добавить функцию получения сообщений по чату @chatId", connection);
        command.Parameters.AddWithValue("chatId", chatId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new Message{
                Id = reader.GetInt32(0),
                ChatId = reader.GetInt32(1),
                SenderId = reader.GetInt32(2),
                Content = reader.GetString(3),
                Timestamp = reader.GetDateTime(4),
                IsRead = reader.GetBoolean(5),
            });
        }

        return messages;
    }
}