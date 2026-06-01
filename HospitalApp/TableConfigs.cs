namespace HospitalApp;

public static class TableConfigs
{
    public static List<TableConfig> All => new()
    {
        Departments(),
        Doctors(),
        Therapists(),
        Surgeons(),
        Patients(),
        VisitRecords(),
        Rooms(),
        Appointments(),
        DoctorConsultations()
    };

    private static ColumnConfig Text(
        string name,
        string label,
        bool pk = false,
        bool required = true,
        int? minLength = null,
        int? maxLength = null,
        string? regexPattern = null,
        string? regexErrorMessage = null)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Text,
            IsPrimaryKey = pk,
            IsRequired = required,
            MinLength = minLength,
            MaxLength = maxLength,
            RegexPattern = regexPattern,
            RegexErrorMessage = regexErrorMessage
        };
    }

    private static ColumnConfig Int(
        string name,
        string label,
        bool pk = false,
        bool required = true,
        int? minValue = null,
        int? maxValue = null)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Integer,
            IsPrimaryKey = pk,
            IsRequired = required,
            MinValue = minValue,
            MaxValue = maxValue
        };
    }

    private static ColumnConfig Date(
        string name,
        string label,
        bool pk = false,
        bool required = true,
        DateTime? minDate = null,
        bool maxDateIsToday = false,
        bool minDateIsTomorrow = false)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Date,
            IsPrimaryKey = pk,
            IsRequired = required,
            MinDate = minDate,
            MaxDateIsToday = maxDateIsToday,
            MinDateIsTomorrow = minDateIsTomorrow
        };
    }

    private static ColumnConfig Time(string name, string label, bool pk = false, bool required = true)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Time,
            IsPrimaryKey = pk,
            IsRequired = required
        };
    }

    private static ColumnConfig Fk(
        string name,
        string label,
        string sql,
        string valueMember,
        string displayMember,
        bool pk = false,
        bool required = true)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.ForeignKey,
            IsPrimaryKey = pk,
            IsRequired = required,
            Lookup = new LookupConfig
            {
                Sql = sql,
                ValueMember = valueMember,
                DisplayMember = displayMember
            }
        };
    }

    private static string DoctorsLookupSql => """
        SELECT
            passport,
            passport || ' — ' || last_name || ' ' || first_name || COALESCE(' ' || middle_name, '') AS full_name
        FROM doctors
        ORDER BY last_name, first_name, middle_name
        """;

    private static string DepartmentsLookupSql => """
        SELECT name
        FROM departments
        ORDER BY name
        """;

    private static string PatientsLookupSql => """
        SELECT
            card_number,
            card_number || ' — ' || last_name || ' ' || first_name || COALESCE(' ' || middle_name, '') AS full_name
        FROM patients
        ORDER BY last_name, first_name, middle_name
        """;

    private static string RoomsLookupSql => """
        SELECT
            number,
            number || ' — ' || equipment_type AS room_label
        FROM rooms
        ORDER BY number
        """;

    private static TableConfig Departments()
    {
        return new TableConfig
        {
            Title = "Відділення",
            TableName = "departments",
            SelectSql = """
                SELECT
                    dep.name,
                    dep.floor,
                    dep.head_passport,
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS head_full_name
                FROM departments dep
                JOIN doctors d ON d.passport = dep.head_passport
                ORDER BY dep.name
                """,
            HiddenGridColumns = new List<string> { "head_passport" },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["name"] = "Назва",
                ["floor"] = "Поверх",
                ["head_full_name"] = "Завідувач"
            },
            Columns = new List<ColumnConfig>
            {
                Text("name", "Назва", pk: true, minLength: 2, maxLength: 100),
                Int("floor", "Поверх", minValue: 1, maxValue: 200),
                Fk("head_passport", "Завідувач", DoctorsLookupSql, "passport", "full_name")
            }
        };
    }

    private static TableConfig Doctors()
    {
        return new TableConfig
        {
            Title = "Лікарі",
            TableName = "doctors",
            SelectSql = """
                SELECT passport, last_name, first_name, middle_name, specialization, department_name
                FROM doctors
                ORDER BY passport
                """,
            ColumnHeaders = new Dictionary<string, string>
            {
                ["passport"] = "Паспорт",
                ["last_name"] = "Прізвище",
                ["first_name"] = "Ім'я",
                ["middle_name"] = "По батькові",
                ["specialization"] = "Спеціалізація",
                ["department_name"] = "Відділення"
            },
            Columns = new List<ColumnConfig>
            {
                Text("passport", "Паспорт", pk: true, minLength: 3, maxLength: 20),
                Text("last_name", "Прізвище", minLength: 2, maxLength: 50),
                Text("first_name", "Ім'я", minLength: 2, maxLength: 50),
                Text("middle_name", "По батькові", required: false, minLength: 2, maxLength: 50),
                Text("specialization", "Спеціалізація", minLength: 2, maxLength: 100),
                Fk("department_name", "Відділення", DepartmentsLookupSql, "name", "name")
            }
        };
    }

    private static TableConfig Therapists()
    {
        return new TableConfig
        {
            Title = "Терапевти",
            TableName = "therapists",
            SelectSql = """
                SELECT
                    t.doctor_passport,
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS doctor_full_name,
                    t.districts_count
                FROM therapists t
                JOIN doctors d ON d.passport = t.doctor_passport
                ORDER BY d.last_name, d.first_name
                """,
            HiddenGridColumns = new List<string> { "doctor_passport" },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["doctor_full_name"] = "Лікар",
                ["districts_count"] = "Кількість дільниць"
            },
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Int("districts_count", "Кількість дільниць", minValue: 0, maxValue: 100)
            }
        };
    }

    private static TableConfig Surgeons()
    {
        return new TableConfig
        {
            Title = "Хірурги",
            TableName = "surgeons",
            SelectSql = """
                SELECT
                    s.doctor_passport,
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS doctor_full_name,
                    s.category
                FROM surgeons s
                JOIN doctors d ON d.passport = s.doctor_passport
                ORDER BY d.last_name, d.first_name
                """,
            HiddenGridColumns = new List<string> { "doctor_passport" },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["doctor_full_name"] = "Лікар",
                ["category"] = "Категорія"
            },
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Text("category", "Категорія операцій", minLength: 2, maxLength: 100)
            }
        };
    }

    private static TableConfig Patients()
    {
        return new TableConfig
        {
            Title = "Пацієнти",
            TableName = "patients",
            SelectSql = """
                SELECT card_number, last_name, first_name, middle_name, birth_date
                FROM patients
                ORDER BY card_number
                """,
            ColumnHeaders = new Dictionary<string, string>
            {
                ["card_number"] = "Номер картки",
                ["last_name"] = "Прізвище",
                ["first_name"] = "Ім'я",
                ["middle_name"] = "По батькові",
                ["birth_date"] = "Дата народження"
            },
            Columns = new List<ColumnConfig>
            {
                Text("card_number", "Номер картки", pk: true, minLength: 3, maxLength: 20),
                Text("last_name", "Прізвище", minLength: 2, maxLength: 50),
                Text("first_name", "Ім'я", minLength: 2, maxLength: 50),
                Text("middle_name", "По батькові", required: false, minLength: 2, maxLength: 50),
                Date("birth_date", "Дата народження", minDate: new DateTime(1900, 1, 1), maxDateIsToday: true)
            }
        };
    }

    private static TableConfig VisitRecords()
    {
        return new TableConfig
        {
            Title = "Записи візитів",
            TableName = "visit_records",
            SelectSql = """
                SELECT
                    vr.patient_card_number,
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS patient_full_name,
                    vr.visit_date,
                    vr.visit_time,
                    vr.complaints
                FROM visit_records vr
                JOIN patients p ON p.card_number = vr.patient_card_number
                ORDER BY vr.patient_card_number, vr.visit_date, vr.visit_time
                """,
            HiddenGridColumns = new List<string> { "patient_card_number" },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["patient_full_name"] = "Пацієнт",
                ["visit_date"] = "Дата",
                ["visit_time"] = "Час",
                ["complaints"] = "Скарги"
            },
            Columns = new List<ColumnConfig>
            {
                Fk("patient_card_number", "Пацієнт", PatientsLookupSql, "card_number", "full_name", pk: true),
                Date("visit_date", "Дата візиту", pk: true, minDateIsTomorrow: true),
                Time("visit_time", "Час візиту", pk: true),
                Text("complaints", "Скарги", required: false, maxLength: 500)
            }
        };
    }

    private static TableConfig Rooms()
    {
        return new TableConfig
        {
            Title = "Кабінети",
            TableName = "rooms",
            SelectSql = """
                SELECT number, equipment_type
                FROM rooms
                ORDER BY number
                """,
            ColumnHeaders = new Dictionary<string, string>
            {
                ["number"] = "Номер",
                ["equipment_type"] = "Обладнання"
            },
            Columns = new List<ColumnConfig>
            {
                Text("number", "Номер кабінету", pk: true, minLength: 1, maxLength: 20),
                Text("equipment_type", "Обладнання", minLength: 2, maxLength: 255)
            }
        };
    }

    private static TableConfig Appointments()
    {
        return new TableConfig
        {
            Title = "Прийоми",
            TableName = "appointments",
            SelectSql = """
                SELECT
                    a.doctor_passport,
                    d.last_name || ' ' || d.first_name || COALESCE(' ' || d.middle_name, '') AS doctor_full_name,
                    a.patient_card_number,
                    p.last_name || ' ' || p.first_name || COALESCE(' ' || p.middle_name, '') AS patient_full_name,
                    a.room_number,
                    r.number || ' — ' || r.equipment_type AS room_label
                FROM appointments a
                JOIN doctors d ON d.passport = a.doctor_passport
                JOIN patients p ON p.card_number = a.patient_card_number
                JOIN rooms r ON r.number = a.room_number
                ORDER BY a.doctor_passport, a.patient_card_number, a.room_number
                """,
            HiddenGridColumns = new List<string>
            {
                "doctor_passport",
                "patient_card_number",
                "room_number"
            },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["doctor_full_name"] = "Лікар",
                ["patient_full_name"] = "Пацієнт",
                ["room_label"] = "Кабінет"
            },
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Fk("patient_card_number", "Пацієнт", PatientsLookupSql, "card_number", "full_name", pk: true),
                Fk("room_number", "Кабінет", RoomsLookupSql, "number", "room_label", pk: true)
            }
        };
    }

    private static TableConfig DoctorConsultations()
    {
        return new TableConfig
        {
            Title = "Консультації лікарів",
            TableName = "doctor_consultations",
            SelectSql = """
                SELECT
                    dc.doctor_passport,
                    d1.last_name || ' ' || d1.first_name || COALESCE(' ' || d1.middle_name, '') AS doctor_full_name,
                    dc.consultant_passport,
                    d2.last_name || ' ' || d2.first_name || COALESCE(' ' || d2.middle_name, '') AS consultant_full_name
                FROM doctor_consultations dc
                JOIN doctors d1 ON d1.passport = dc.doctor_passport
                JOIN doctors d2 ON d2.passport = dc.consultant_passport
                ORDER BY d1.last_name, d1.first_name, d2.last_name, d2.first_name
                """,
            HiddenGridColumns = new List<string>
            {
                "doctor_passport",
                "consultant_passport"
            },
            ColumnHeaders = new Dictionary<string, string>
            {
                ["doctor_full_name"] = "Лікар",
                ["consultant_full_name"] = "Лікар-консультант"
            },
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Fk("consultant_passport", "Лікар-консультант", DoctorsLookupSql, "passport", "full_name", pk: true)
            }
        };
    }
}
