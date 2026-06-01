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
            last_name,
            first_name,
            middle_name
        FROM doctors
        ORDER BY last_name, first_name, middle_name
        """;

    private static string RoomsLookupSql => """
        SELECT number
        FROM rooms
        ORDER BY number
        """;

    public static List<QueryConfig> All => new()
    {
    DoctorsByDepartment(),
    PatientsWithVisitsAfterDate(),
    AppointmentsByDepartmentAndRoom(),
    ConsultationsByDoctorDepartment(),
    PatientsBornBeforeDateWithVisitCount(),
    DoctorsWithSamePatientsAsSelectedDoctor(),
    PatientsSeenByAllDoctorsInDepartment()//, task()
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
                d.last_name AS "Прізвище",
                d.first_name AS "Ім'я",
                d.middle_name AS "По батькові",
                d.specialization AS "Спеціалізація"
            FROM doctors d
            INNER JOIN departments dep ON dep.name = d.department_name
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
            Description = "Вивести пацієнтів та їхні записи візитів, дата яких не раніше заданої.",
            Parameters = new List<QueryParameter>
        {
            DateParameter("start_date", "Дата початку")
        },
            Sql = """
            SELECT
                p.card_number AS "Номер картки",
                p.last_name AS "Прізвище",
                p.first_name AS "Ім'я",
                p.middle_name AS "По батькові",
                p.birth_date AS "Дата народження",
                vr.visit_date AS "Дата візиту",
                vr.visit_time AS "Час візиту",
                vr.complaints AS "Скарги"
            FROM patients p
            INNER JOIN visit_records vr ON vr.patient_card_number = p.card_number
            WHERE vr.visit_date >= @start_date
            ORDER BY vr.visit_date, vr.visit_time, p.last_name;
            """
        };
    }

    private static QueryConfig AppointmentsByDepartmentAndRoom()
    {
        return new QueryConfig
        {
            Title = "3. Прийоми лікарів заданого відділення у заданому кабінеті",
            Description = "Вивести прийоми, у яких лікар працює у вибраному відділенні, а прийом проходить у вибраному кабінеті.",
            Parameters = new List<QueryParameter>
        {
            ForeignKeyParameter("department_name", "Відділення", DepartmentsLookupSql, "name", "name"),
            ForeignKeyParameter("room_number", "Кабінет", RoomsLookupSql, "number", "number")
        },
            Sql = """
            SELECT
                d.last_name AS "Прізвище лікаря",
                d.first_name AS "Ім'я лікаря",
                d.middle_name AS "По батькові лікаря",
                d.specialization AS "Спеціалізація",
                dep.name AS "Відділення",
                p.card_number AS "Номер картки",
                p.last_name AS "Прізвище пацієнта",
                p.first_name AS "Ім'я пацієнта",
                p.middle_name AS "По батькові пацієнта",
                r.number AS "Кабінет",
                r.equipment_type AS "Обладнання"
            FROM appointments a
            INNER JOIN doctors d ON d.passport = a.doctor_passport
            INNER JOIN departments dep ON dep.name = d.department_name
            INNER JOIN patients p ON p.card_number = a.patient_card_number
            INNER JOIN rooms r ON r.number = a.room_number
            WHERE dep.name = @department_name
              AND r.number = @room_number
            ORDER BY d.last_name, d.first_name, p.last_name, p.first_name;
            """
        };
    }

    private static QueryConfig ConsultationsByDoctorDepartment()
    {
        return new QueryConfig
        {
            Title = "4. Консультації лікарів заданого відділення",
            Description = "Вивести консультації, у яких основний лікар працює у вибраному відділенні.",
            Parameters = new List<QueryParameter>
        {
            ForeignKeyParameter("department_name", "Відділення лікаря", DepartmentsLookupSql, "name", "name")
        },
            Sql = """
            SELECT
                d1.last_name AS "Прізвище лікаря",
                d1.first_name AS "Ім'я лікаря",
                d1.middle_name AS "По батькові лікаря",
                d1.specialization AS "Спеціалізація лікаря",
                dep1.name AS "Відділення лікаря",
                d2.last_name AS "Прізвище консультанта",
                d2.first_name AS "Ім'я консультанта",
                d2.middle_name AS "По батькові консультанта",
                d2.specialization AS "Спеціалізація консультанта",
                dep2.name AS "Відділення консультанта"
            FROM doctor_consultations dc
            INNER JOIN doctors d1 ON d1.passport = dc.doctor_passport
            INNER JOIN departments dep1 ON dep1.name = d1.department_name
            INNER JOIN doctors d2 ON d2.passport = dc.consultant_passport
            INNER JOIN departments dep2 ON dep2.name = d2.department_name
            WHERE dep1.name = @department_name
            ORDER BY d1.last_name, d1.first_name, d2.last_name, d2.first_name;
            """
        };
    }

    private static QueryConfig PatientsBornBeforeDateWithVisitCount()
    {
        return new QueryConfig
        {
            Title = "5. Пацієнти, народжені не пізніше заданої дати, із записами візитів",
            Description = "Вивести пацієнтів, дата народження яких не пізніша за задану, і кількість їхніх записів візитів.",
            Parameters = new List<QueryParameter>
        {
            DateParameter("max_birth_date", "Дата народження не пізніше")
        },
            Sql = """
            SELECT
                p.card_number AS "Номер картки",
                p.last_name AS "Прізвище",
                p.first_name AS "Ім'я",
                p.middle_name AS "По батькові",
                p.birth_date AS "Дата народження",
                COUNT(vr.visit_date) AS "Кількість записів візитів"
            FROM patients p
            INNER JOIN visit_records vr ON vr.patient_card_number = p.card_number
            WHERE p.birth_date <= @max_birth_date
            GROUP BY p.card_number, p.last_name, p.first_name, p.middle_name, p.birth_date
            ORDER BY p.birth_date, p.last_name, p.first_name;
            """
        };
    }

    private static QueryConfig DoctorsWithSamePatientsAsSelectedDoctor()
    {
        return new QueryConfig
        {
            Title = "6. Лікарі з таким самим набором пацієнтів, як заданий лікар",
            Description = "Вивести лікарів, які мають точно таку саму множину пацієнтів у таблиці прийомів, як вибраний лікар.",
            Parameters = new List<QueryParameter>
        {
            ForeignKeyParameter("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "doctor_display")
        },
            Sql = """
            SELECT
                d.passport AS "Паспорт лікаря",
                d.last_name AS "Прізвище",
                d.first_name AS "Ім'я",
                d.middle_name AS "По батькові",
                d.specialization AS "Спеціалізація"
            FROM doctors d
            WHERE NOT d.passport = @doctor_passport
              AND EXISTS (
                  SELECT a0.patient_card_number
                  FROM appointments a0
                  WHERE a0.doctor_passport = @doctor_passport
              )
              AND NOT EXISTS (
                  SELECT a1.patient_card_number
                  FROM appointments a1
                  WHERE a1.doctor_passport = @doctor_passport
                    AND NOT EXISTS (
                        SELECT a2.patient_card_number
                        FROM appointments a2
                        WHERE a2.doctor_passport = d.passport
                          AND a2.patient_card_number = a1.patient_card_number
                    )
              )
              AND NOT EXISTS (
                  SELECT a3.patient_card_number
                  FROM appointments a3
                  WHERE a3.doctor_passport = d.passport
                    AND NOT EXISTS (
                        SELECT a4.patient_card_number
                        FROM appointments a4
                        WHERE a4.doctor_passport = @doctor_passport
                          AND a4.patient_card_number = a3.patient_card_number
                    )
              )
            ORDER BY d.last_name, d.first_name, d.middle_name;
            """
        };
    }

    private static QueryConfig PatientsSeenByAllDoctorsInDepartment()
    {
        return new QueryConfig
        {
            Title = "7. Пацієнти, які були на прийомі у всіх лікарів заданого відділення",
            Description = "Вивести пацієнтів, які мали прийом у кожного лікаря вибраного відділення.",
            Parameters = new List<QueryParameter>
        {
            ForeignKeyParameter("department_name", "Відділення", DepartmentsLookupSql, "name", "name")
        },
            Sql = """
            SELECT
                p.card_number AS "Номер картки",
                p.last_name AS "Прізвище",
                p.first_name AS "Ім'я",
                p.middle_name AS "По батькові",
                p.birth_date AS "Дата народження"
            FROM patients p
            WHERE EXISTS (
                SELECT d0.passport
                FROM doctors d0
                WHERE d0.department_name = @department_name
            )
              AND NOT EXISTS (
                  SELECT d1.passport
                  FROM doctors d1
                  WHERE d1.department_name = @department_name
                    AND NOT EXISTS (
                        SELECT a.patient_card_number
                        FROM appointments a
                        WHERE a.patient_card_number = p.card_number
                          AND a.doctor_passport = d1.passport
                    )
              )
            ORDER BY p.last_name, p.first_name, p.middle_name;
            """
        };
    }
    private static QueryConfig task()
    {
        return new QueryConfig
        {
            Title = "8. ",
            Description = "",
            Parameters = new List<QueryParameter>
        {
            //ForeignKeyParameter("department_name", "Відділення", DepartmentsLookupSql, "name", "name")
        },
            Sql = """
            SELECT
                
                ;
            """
        };
    }
}
