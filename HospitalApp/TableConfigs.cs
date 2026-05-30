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

    private static ColumnConfig Text(string name, string label, bool pk = false, bool required = true)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Text,
            IsPrimaryKey = pk,
            IsRequired = required
        };
    }

    private static ColumnConfig Int(string name, string label, bool pk = false, bool required = true)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Integer,
            IsPrimaryKey = pk,
            IsRequired = required
        };
    }

    private static ColumnConfig Date(string name, string label, bool pk = false, bool required = true)
    {
        return new ColumnConfig
        {
            Name = name,
            Label = label,
            Kind = FieldKind.Date,
            IsPrimaryKey = pk,
            IsRequired = required
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
                SELECT name, floor, head_passport
                FROM departments
                ORDER BY name
                """,

            Columns = new List<ColumnConfig>
            {
                Text("name", "Назва", pk: true),
                Int("floor", "Поверх"),
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
            Columns = new List<ColumnConfig>
            {
                Text("passport", "Паспорт", pk: true),
                Text("last_name", "Прізвище"),
                Text("first_name", "Ім'я"),
                Text("middle_name", "По батькові", required: false),
                Text("specialization", "Спеціалізація"),
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
                SELECT doctor_passport, districts_count
                FROM therapists
                ORDER BY doctor_passport
                """,
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Int("districts_count", "Кількість дільниць")
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
                SELECT doctor_passport, category
                FROM surgeons
                ORDER BY doctor_passport
                """,
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Text("category", "Категорія операцій")
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
            Columns = new List<ColumnConfig>
            {
                Text("card_number", "Номер картки", pk: true),
                Text("last_name", "Прізвище"),
                Text("first_name", "Ім'я"),
                Text("middle_name", "По батькові", required: false),
                Date("birth_date", "Дата народження")
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
                SELECT patient_card_number, visit_date, visit_time, complaints
                FROM visit_records
                ORDER BY patient_card_number, visit_date, visit_time
                """,
            Columns = new List<ColumnConfig>
            {
                Fk("patient_card_number", "Пацієнт", PatientsLookupSql, "card_number", "full_name", pk: true),
                Date("visit_date", "Дата візиту", pk: true),
                Time("visit_time", "Час візиту", pk: true),
                Text("complaints", "Скарги", required: false)
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
            Columns = new List<ColumnConfig>
            {
                Text("number", "Номер кабінету", pk: true),
                Text("equipment_type", "Обладнання")
            }
        };
    }

    private static TableConfig Appointments()
    {
        return new TableConfig
        {
            Title = "Прийоми / процедури",
            TableName = "appointments",
            SelectSql = """
                SELECT doctor_passport, patient_card_number, room_number
                FROM appointments
                ORDER BY doctor_passport, patient_card_number, room_number
                """,
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
                SELECT doctor_passport, consultant_passport
                FROM doctor_consultations
                ORDER BY doctor_passport, consultant_passport
                """,
            Columns = new List<ColumnConfig>
            {
                Fk("doctor_passport", "Лікар", DoctorsLookupSql, "passport", "full_name", pk: true),
                Fk("consultant_passport", "Лікар-консультант", DoctorsLookupSql, "passport", "full_name", pk: true)
            }
        };
    }
}
