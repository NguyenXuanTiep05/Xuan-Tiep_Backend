using MySqlConnector;

using Auth.Src.Models;


namespace Auth.Src.Services;

public static class Password
{
    private static string _connectionString = "";
    public static void SetConn(string conn) { _connectionString = conn; }
    public static bool Compare(LoginRequest lgr, out int? id, out string? role)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var command = new MySqlCommand("Select password, id, role from users where username = @username and is_active = 1", connection);
        command.Parameters.Add("@username", MySqlDbType.VarChar).Value = lgr.Username.ToLower();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            string storedHash = reader.GetString("password");

            bool isValid = BCrypt.Net.BCrypt.Verify(lgr.Password, storedHash);
            id = reader.GetInt32("id");
            role = reader.GetString("role");
            return isValid;

        }
        id = null;
        role = null;
        return false;
    }
}
