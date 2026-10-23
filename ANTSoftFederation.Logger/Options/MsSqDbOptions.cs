namespace SorteFederation.Logger.Options;
public class MsSqDbOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }
    public string TableName { get; set; }
    public string SchemaName { get; set; }
}
