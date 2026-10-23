namespace SorteFederation.Logger.Options;

public class MongoDbOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }
    public string Database { get; set; }
    public string CollectionName { get; set; }
}
