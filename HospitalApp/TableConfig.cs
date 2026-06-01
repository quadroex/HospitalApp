namespace HospitalApp;

public enum FieldKind
{
    Text,
    Integer,
    Date,
    Time,
    ForeignKey
}

public sealed class LookupConfig
{
    public string Sql { get; set; } = "";
    public string ValueMember { get; set; } = "";
    public string DisplayMember { get; set; } = "";
}

public sealed class ColumnConfig
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public FieldKind Kind { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsRequired { get; set; } = true;
    public LookupConfig? Lookup { get; set; }
}

public sealed class TableConfig
{
    public string Title { get; set; } = "";
    public string TableName { get; set; } = "";
    public string SelectSql { get; set; } = "";
    public bool AllowAdd { get; set; } = true;
    public bool AllowEdit { get; set; } = true;
    public bool AllowDelete { get; set; } = true;

    public List<ColumnConfig> Columns { get; set; } = new();
    public List<string> HiddenGridColumns { get; set; } = new();
    public Dictionary<string, string> ColumnHeaders { get; set; } = new();

    public IEnumerable<ColumnConfig> PrimaryKeys =>
        Columns.Where(column => column.IsPrimaryKey);
}
