using System.Data;
using Npgsql;

namespace HospitalApp;

public static class Db
{
    private static readonly string[] ConnectionStrings =
    {
        BuildConnectionString(5433),
        BuildConnectionString(5432)
    };

    public static DataTable GetTable(string sql, params NpgsqlParameter[] parameters)
    {
        return RunWithFallback(connectionString =>
        {
            using var connection = new NpgsqlConnection(connectionString);
            using var command = new NpgsqlCommand(sql, connection);

            if (parameters.Length > 0)
                command.Parameters.AddRange(CloneParameters(parameters));

            using var adapter = new NpgsqlDataAdapter(command);
            var table = new DataTable();

            adapter.Fill(table);
            return table;
        });
    }

    public static int Execute(string sql, params NpgsqlParameter[] parameters)
    {
        return RunWithFallback(connectionString =>
        {
            using var connection = new NpgsqlConnection(connectionString);
            using var command = new NpgsqlCommand(sql, connection);

            if (parameters.Length > 0)
                command.Parameters.AddRange(CloneParameters(parameters));

            connection.Open();
            return command.ExecuteNonQuery();
        });
    }

    public static NpgsqlConnection OpenConnection()
    {
        return RunWithFallback(connectionString =>
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        });
    }

    public static string GetFriendlyError(Exception ex)
    {
        if (ex is PostgresException pgEx)
        {
            return pgEx.SqlState switch
            {
                "23503" => "Foreign key violation. Check the selected related records, or delete dependent records first.",
                "23505" => "Unique violation. A record with this key already exists.",
                "23514" => "Check constraint violation. The entered data does not pass validation.",
                "23502" => "Not-null violation. A required field is empty.",
                _ => pgEx.MessageText
            };
        }

        if (ex is NpgsqlException)
            return "Could not connect to PostgreSQL or execute the command. Check that the server is running, database hospital exists, and port 5433 or 5432 is available.";

        return ex.Message;
    }

    private static string BuildConnectionString(int port)
    {
        return $"Host=localhost;Port={port};Database=hospital;Username=postgres;Password=1234";
    }

    private static NpgsqlParameter[] CloneParameters(NpgsqlParameter[] parameters)
    {
        return parameters
            .Select(parameter => new NpgsqlParameter(parameter.ParameterName, parameter.Value ?? DBNull.Value))
            .ToArray();
    }

    private static T RunWithFallback<T>(Func<string, T> action)
    {
        Exception? firstConnectionError = null;

        foreach (var connectionString in ConnectionStrings)
        {
            try
            {
                return action(connectionString);
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (NpgsqlException ex) when (firstConnectionError == null)
            {
                firstConnectionError = ex;
            }
        }

        throw firstConnectionError ?? new InvalidOperationException("Could not connect to the database.");
    }
}
