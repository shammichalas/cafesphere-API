namespace CafeSphere.Persistence.Configurations;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "CafeSphereDb";
    public bool AutoCreateIndexes { get; set; } = true;
    public int ServerSelectionTimeoutSeconds { get; set; } = 3;
}
