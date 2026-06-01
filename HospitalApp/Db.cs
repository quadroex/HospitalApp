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
                "23503" => GetForeignKeyMessage(pgEx.ConstraintName),
                "23505" => GetUniqueMessage(pgEx.ConstraintName),
                "23514" => GetCheckMessage(pgEx.ConstraintName),
                "23502" => "Обов'язкове поле не заповнене.",
                _ => pgEx.MessageText
            };
        }

        if (ex is NpgsqlException)
            return "Не вдалося підключитися до PostgreSQL. Перевірте, що сервер запущений, база hospital існує, а порт 5433 або 5432 доступний.";

        return ex.Message;
    }

    private static string GetForeignKeyMessage(string? constraintName)
    {
        return constraintName switch
        {
            "fk_department_head" => "Завідувач має працювати у цьому ж відділенні.",
            "fk_doctor_department" => "Неможливо виконати операцію: відділення не існує або у відділенні вже є пов'язані лікарі.",
            "fk_appointment_doctor" => "Неможливо видалити лікаря: для нього існують прийоми, або вибрано неіснуючого лікаря.",
            "fk_appointment_patient" => "Неможливо видалити пацієнта: для нього існують прийоми, або вибрано неіснуючого пацієнта.",
            "fk_appointment_room" => "Неможливо видалити кабінет: для нього існують прийоми, або вибрано неіснуючий кабінет.",
            "fk_therapist_doctor" => "Неможливо виконати операцію: запис терапевта пов'язаний з лікарем.",
            "fk_surgeon_doctor" => "Неможливо виконати операцію: запис хірурга пов'язаний з лікарем.",
            "fk_consultation_doctor" or "fk_consultation_consultant" => "Неможливо виконати операцію: консультація пов'язана з лікарем.",
            "fk_visit_record_patient" => "Неможливо виконати операцію: запис візиту пов'язаний з пацієнтом.",
            _ => "Неможливо виконати операцію: запис пов'язаний з іншими таблицями або вибрано неіснуюче пов'язане значення."
        };
    }

    private static string GetUniqueMessage(string? constraintName)
    {
        return constraintName switch
        {
            "appointments_pkey" => "Такий прийом уже існує.",
            "doctor_consultations_pkey" => "Така консультація вже існує.",
            "departments_head_passport_key" => "Цей лікар уже є завідувачем іншого відділення.",
            _ => "Такий запис уже існує."
        };
    }

    private static string GetCheckMessage(string? constraintName)
    {
        return constraintName switch
        {
            "chk_doctor_consultation_not_self" => "Лікар не може консультувати сам себе.",
            "departments_floor_check" => "Поверх має бути додатним числом.",
            "therapists_districts_count_check" => "Кількість дільниць не може бути від'ємною.",
            _ => "Дані не проходять перевірку обмежень бази даних."
        };
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
