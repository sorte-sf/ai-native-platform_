
using SorteFederation.Logger.Options;
using FileOptions = SorteFederation.Logger.Options.FileOptions;

namespace SorteFederation.Logger;

public class LoggerOptions
{
    public string Level { get; set; }

    public ConsoleOptions Console { get; set; }

    public FileOptions File { get; set; }

    public ElkOptions Elk { get; set; }

    public SeqOptions Seq { get; set; }

    public LokiOptions Loki { get; set; }

    public ELKLogstashOptions ElkLog { get; set; }

    public MongoDbOptions Mongodb { get; set; }

    public MsSqDbOptions Mssqldb { get; set; }

    public SendMailOptions SendMail { get; set; }

    //public KafkaOptions KafkaOptions { get; set; }

    public IDictionary<string, string> MinimumLevelOverrides { get; set; }

    public IEnumerable<string> ExcludePaths { get; set; }

    public IEnumerable<string> ExcludeProperties { get; set; }

    public IDictionary<string, object> Tags { get; set; } 
}