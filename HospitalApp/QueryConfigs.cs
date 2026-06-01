namespace HospitalApp;

public static class QueryConfigs
{
    private static string DepartmentsLookupSql => """
        SELECT name
        FROM departments
        ORDER BY name
        """;

    private static string DoctorsLookupSql => """
        SELECT
            passport,
            passport || ' - ' || last_name || ' ' || first_name || COALESCE(' ' || middle_name, '') AS full_name
        FROM doctors
        ORDER BY last_name, first_name, middle_name
        """;

    public static List<QueryConfig> All => new()
    {
        DoctorsByDepartment(),
        PatientsWithVisitsAfterDate(),
        AppointmentsByEquipment(),
        PatientsByDoctorSpecialization(),
        OlderPatientsWithVisitCount(),
        DoctorsWithSamePatientsAsSelectedDoctor(),
        DoctorPairsWithSamePatients()
    };

    private static QueryParameter TextParameter(string name, string label)
    {
        return new QueryParameter
        {
            Name = name,
            Label = label,
            Kind = QueryParameterKind.Text
        };
    }

    private static QueryParameter IntegerParameter(string name, string label)
    {
        return new QueryParameter
        {
            Name = name,
            Label = label,
            Kind = QueryParameterKind.Integer
        };
    }

    private static QueryParameter DateParameter(string name, string label)
    {
        return new QueryParameter
        {
            Name = name,
            Label = label,
            Kind = QueryParameterKind.Date
        };
    }

    private static QueryParameter ForeignKeyParameter(
        string name,
        string label,
        string sql,
        string valueMember,
        string displayMember)
    {
        return new QueryParameter
        {
            Name = name,
            Label = label,
            Kind = QueryParameterKind.ForeignKey,
            Lookup = new LookupConfig
            {
                Sql = sql,
                ValueMember = valueMember,
                DisplayMember = displayMember
            }
        };
    }

    private static QueryConfig DoctorsByDepartment()
    {
        return new QueryConfig
        {
            Title = "1. Лікарі заданого відділення",
            Description = "Вивести лікарів, які працюють у вибраному відділенні.",
            Parameters = new List<QueryParameter>
            {
                ForeignKeyParameter("department_name", "Відділення", DepartmentsLookupSql, "name", "name")
            },
            Sql = """
                SELECT
                    d.passport AS "Паспорт лікаря",
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS "ПІБ лікаря",
                    d.specialization AS "Спеціалізація",
                    dep.name AS "Відділення",
                    dep.floor AS "Поверх"
                FROM doctors d
                JOIN departments dep ON dep.name = d.department_name
                WHERE dep.name = @department_name
                ORDER BY d.last_name, d.first_name, d.middle_name;
                """
        };
    }

    private static QueryConfig PatientsWithVisitsAfterDate()
    {
        return new QueryConfig
        {
            Title = "2. Пацієнти з візитами після заданої дати",
            Description = "Вивести пацієнтів та їхні записи візитів, дата яких не раніше заданої користувачем.",
            Parameters = new List<QueryParameter>
            {
                DateParameter("start_date", "Дата початку")
            },
            Sql = """
                SELECT
                    p.card_number AS "Номер картки",
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS "ПІБ пацієнта",
                    p.birth_date AS "Дата народження",
                    vr.visit_date AS "Дата візиту",
                    vr.visit_time AS "Час візиту",
                    vr.complaints AS "Скарги"
                FROM patients p
                JOIN visit_records vr ON vr.patient_card_number = p.card_number
                WHERE vr.visit_date >= @start_date
                ORDER BY vr.visit_date, vr.visit_time, p.last_name;
                """
        };
    }

    private static QueryConfig AppointmentsByEquipment()
    {
        return new QueryConfig
        {
            Title = "3. Прийоми у кабінетах із заданим обладнанням",
            Description = "Вивести прийоми або процедури, що проводяться в кабінетах, обладнання яких містить введений текст.",
            Parameters = new List<QueryParameter>
            {
                TextParameter("equipment_text", "Текст обладнання")
            },
            Sql = """
                SELECT
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS "Лікар",
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS "Пацієнт",
                    r.number AS "Кабінет",
                    r.equipment_type AS "Обладнання"
                FROM appointments a
                JOIN doctors d ON d.passport = a.doctor_passport
                JOIN patients p ON p.card_number = a.patient_card_number
                JOIN rooms r ON r.number = a.room_number
                WHERE LOWER(r.equipment_type) LIKE '%' || LOWER(@equipment_text) || '%'
                ORDER BY r.number, d.last_name, p.last_name;
                """
        };
    }

    private static QueryConfig PatientsByDoctorSpecialization()
    {
        return new QueryConfig
        {
            Title = "4. Пацієнти лікарів заданої спеціалізації",
            Description = "Вивести пацієнтів, які були пов'язані з лікарями заданої спеціалізації.",
            Parameters = new List<QueryParameter>
            {
                TextParameter("specialization_text", "Текст спеціалізації")
            },
            Sql = """
                SELECT DISTINCT
                    p.card_number AS "Номер картки",
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS "Пацієнт",
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS "Лікар",
                    d.specialization AS "Спеціалізація"
                FROM patients p
                JOIN appointments a ON a.patient_card_number = p.card_number
                JOIN doctors d ON d.passport = a.doctor_passport
                WHERE LOWER(d.specialization) LIKE '%' || LOWER(@specialization_text) || '%'
                ORDER BY p.card_number, d.specialization;
                """
        };
    }

    private static QueryConfig OlderPatientsWithVisitCount()
    {
        return new QueryConfig
        {
            Title = "5. Пацієнти старші за заданий вік із записами візитів",
            Description = "Вивести пацієнтів, вік яких не менший за заданий, і кількість їхніх записів візитів.",
            Parameters = new List<QueryParameter>
            {
                IntegerParameter("min_age", "Мінімальний вік")
            },
            Sql = """
                SELECT
                    p.card_number AS "Номер картки",
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS "Пацієнт",
                    p.birth_date AS "Дата народження",
                    EXTRACT(YEAR FROM AGE(CURRENT_DATE, p.birth_date))::int AS "Вік",
                    COUNT(vr.*) AS "Кількість записів візитів"
                FROM patients p
                JOIN visit_records vr ON vr.patient_card_number = p.card_number
                WHERE EXTRACT(YEAR FROM AGE(CURRENT_DATE, p.birth_date))::int >= @min_age
                GROUP BY p.card_number, p.last_name, p.first_name, p.middle_name, p.birth_date
                ORDER BY "Вік" DESC, p.last_name;
                """
        };
    }

    private static QueryConfig DoctorsWithSamePatientsAsSelectedDoctor()
    {
        return new QueryConfig
        {
            Title = "6. Лікарі з таким самим набором пацієнтів, як заданий лікар",
            Description = "Вивести лікарів, які мають точно таку саму множину пацієнтів у таблиці прийомів або процедур, як вибраний лікар.",
            Parameters = new List<QueryParameter>
            {
                ForeignKeyParameter("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name")
            },
            Sql = """
                SELECT
                    d.passport AS "Паспорт лікаря",
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS "Лікар",
                    d.specialization AS "Спеціалізація"
                FROM doctors d
                WHERE d.passport <> @doctor_passport
                AND NOT EXISTS (
                    (
                        SELECT a1.patient_card_number
                        FROM appointments a1
                        WHERE a1.doctor_passport = @doctor_passport
                    )
                    EXCEPT
                    (
                        SELECT a2.patient_card_number
                        FROM appointments a2
                        WHERE a2.doctor_passport = d.passport
                    )
                )
                AND NOT EXISTS (
                    (
                        SELECT a2.patient_card_number
                        FROM appointments a2
                        WHERE a2.doctor_passport = d.passport
                    )
                    EXCEPT
                    (
                        SELECT a1.patient_card_number
                        FROM appointments a1
                        WHERE a1.doctor_passport = @doctor_passport
                    )
                )
                AND EXISTS (
                    SELECT 1
                    FROM appointments ax
                    WHERE ax.doctor_passport = @doctor_passport
                )
                ORDER BY d.last_name, d.first_name;
                """
        };
    }

    private static QueryConfig DoctorPairsWithSamePatients()
    {
        return new QueryConfig
        {
            Title = "7. Пари лікарів з однаковою множиною пацієнтів",
            Description = "Вивести пари лікарів, які мають однакову множину пацієнтів у таблиці прийомів або процедур.",
            Sql = """
                SELECT
                    d1.passport AS "Паспорт лікаря 1",
                    d1.last_name || ' ' || d1.first_name || COALESCE(' ' || d1.middle_name, '') AS "Лікар 1",
                    d2.passport AS "Паспорт лікаря 2",
                    d2.last_name || ' ' || d2.first_name || COALESCE(' ' || d2.middle_name, '') AS "Лікар 2"
                FROM doctors d1
                JOIN doctors d2 ON d1.passport < d2.passport
                WHERE NOT EXISTS (
                    (
                        SELECT a1.patient_card_number
                        FROM appointments a1
                        WHERE a1.doctor_passport = d1.passport
                    )
                    EXCEPT
                    (
                        SELECT a2.patient_card_number
                        FROM appointments a2
                        WHERE a2.doctor_passport = d2.passport
                    )
                )
                AND NOT EXISTS (
                    (
                        SELECT a2.patient_card_number
                        FROM appointments a2
                        WHERE a2.doctor_passport = d2.passport
                    )
                    EXCEPT
                    (
                        SELECT a1.patient_card_number
                        FROM appointments a1
                        WHERE a1.doctor_passport = d1.passport
                    )
                )
                AND EXISTS (
                    SELECT 1
                    FROM appointments ax
                    WHERE ax.doctor_passport = d1.passport
                )
                ORDER BY d1.passport, d2.passport;
                """
        };
    }
}
