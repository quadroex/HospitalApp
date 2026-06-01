using System.Data;
using Npgsql;

namespace HospitalApp;

public static class BusinessValidator
{
    public static void ValidateBeforeInsert(TableConfig config, IReadOnlyDictionary<string, object?> values)
    {
        switch (config.TableName)
        {
            case "doctor_consultations":
                ValidateDoctorConsultation(values, originalDoctorPassport: null, originalConsultantPassport: null);
                break;
            case "appointments":
                ValidateAppointmentDuplicate(values);
                break;
        }
    }

    public static void ValidateBeforeUpdate(TableConfig config, IReadOnlyDictionary<string, object?> values, DataRowView originalRow)
    {
        switch (config.TableName)
        {
            case "departments":
                ValidateDepartmentHead(values);
                break;
            case "doctors":
                ValidateHeadDoctorDepartmentChange(values, originalRow);
                break;
            case "doctor_consultations":
                ValidateDoctorConsultation(
                    values,
                    originalRow.Row["doctor_passport"],
                    originalRow.Row["consultant_passport"]);
                break;
        }
    }

    private static void ValidateDoctorConsultation(
        IReadOnlyDictionary<string, object?> values,
        object? originalDoctorPassport,
        object? originalConsultantPassport)
    {
        var doctorPassport = RequiredString(values, "doctor_passport");
        var consultantPassport = RequiredString(values, "consultant_passport");

        if (doctorPassport == consultantPassport)
            throw new InvalidOperationException("Лікар не може консультувати сам себе.");

        var reverseIsOriginal =
            originalDoctorPassport?.ToString() == consultantPassport &&
            originalConsultantPassport?.ToString() == doctorPassport;

        if (!reverseIsOriginal && Exists(
            """
            SELECT 1
            FROM doctor_consultations
            WHERE doctor_passport = @consultant_passport
              AND consultant_passport = @doctor_passport
            LIMIT 1
            """,
            new NpgsqlParameter("doctor_passport", doctorPassport),
            new NpgsqlParameter("consultant_passport", consultantPassport)))
        {
            throw new InvalidOperationException("Зворотна консультація між цими лікарями вже існує.");
        }
    }

    private static void ValidateAppointmentDuplicate(IReadOnlyDictionary<string, object?> values)
    {
        var doctorPassport = RequiredString(values, "doctor_passport");
        var patientCardNumber = RequiredString(values, "patient_card_number");
        var roomNumber = RequiredString(values, "room_number");

        if (Exists(
            """
            SELECT 1
            FROM appointments
            WHERE doctor_passport = @doctor_passport
              AND patient_card_number = @patient_card_number
              AND room_number = @room_number
            LIMIT 1
            """,
            new NpgsqlParameter("doctor_passport", doctorPassport),
            new NpgsqlParameter("patient_card_number", patientCardNumber),
            new NpgsqlParameter("room_number", roomNumber)))
        {
            throw new InvalidOperationException("Такий прийом уже існує.");
        }
    }

    private static void ValidateDepartmentHead(IReadOnlyDictionary<string, object?> values)
    {
        var departmentName = RequiredString(values, "name");
        var headPassport = RequiredString(values, "head_passport");

        if (!Exists(
            """
            SELECT 1
            FROM doctors
            WHERE passport = @head_passport
              AND department_name = @department_name
            LIMIT 1
            """,
            new NpgsqlParameter("head_passport", headPassport),
            new NpgsqlParameter("department_name", departmentName)))
        {
            throw new InvalidOperationException("Завідувач має працювати у цьому ж відділенні.");
        }
    }

    private static void ValidateHeadDoctorDepartmentChange(IReadOnlyDictionary<string, object?> values, DataRowView originalRow)
    {
        var passport = originalRow.Row["passport"]?.ToString() ?? "";
        var originalDepartment = originalRow.Row["department_name"]?.ToString() ?? "";
        var newDepartment = RequiredString(values, "department_name");

        if (string.Equals(originalDepartment, newDepartment, StringComparison.Ordinal))
            return;

        if (Exists(
            """
            SELECT 1
            FROM departments
            WHERE head_passport = @passport
            LIMIT 1
            """,
            new NpgsqlParameter("passport", passport)))
        {
            throw new InvalidOperationException("Цей лікар є завідувачем відділення. Спочатку змініть завідувача відділення або використайте спеціальну транзакційну операцію.");
        }
    }

    private static bool Exists(string sql, params NpgsqlParameter[] parameters)
    {
        return Db.GetTable(sql, parameters).Rows.Count > 0;
    }

    private static string RequiredString(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var value) || value == null || value == DBNull.Value)
            return "";

        return value.ToString() ?? "";
    }
}
