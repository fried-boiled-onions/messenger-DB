using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace messengerDB
{
    public class DbService
    {
        private readonly string _connectionString;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddUserAsync(string username, string email, string password)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("CALL add_user(@p1, @p2, @p3)", conn);
            cmd.Parameters.AddWithValue("p1", username);
            cmd.Parameters.AddWithValue("p2", email);
            cmd.Parameters.AddWithValue("p3", password);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SendMessageAsync(int chatId, int senderId, string content)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("CALL send_message(@p1, @p2, @p3)", conn);
            cmd.Parameters.AddWithValue("p1", chatId);
            cmd.Parameters.AddWithValue("p2", senderId);
            cmd.Parameters.AddWithValue("p3", content);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<object>> GetNewChatsAsync(int userId)
        {
            var result = new List<object>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM get_new_chats(@p1)", conn);
            cmd.Parameters.AddWithValue("p1", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Предполагаем, что процедура возвращает chat_id
                result.Add(new { ChatId = reader.GetInt32(0) });
            }
            return result;
        }
    }
}