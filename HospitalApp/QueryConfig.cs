namespace HospitalApp;

public enum QueryParameterKind
{
    Text,
    Integer,
    Date,
    ForeignKey
}

public sealed class QueryParameter
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public QueryParameterKind Kind { get; set; }
    public bool IsRequired { get; set; } = true;
    public LookupConfig? Lookup { get; set; }
}

public sealed class QueryConfig
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Sql { get; set; } = "";
    public List<QueryParameter> Parameters { get; set; } = new();
}
