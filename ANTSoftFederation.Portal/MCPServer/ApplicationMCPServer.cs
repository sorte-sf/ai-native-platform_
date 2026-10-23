
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SorteFederation.ANTApplications.MCPServer;

[McpServerToolType]
public class ApplicationMCPServer
{
    private readonly IProjectApplicationService _projectApplicationService; 

    public ApplicationMCPServer(IProjectApplicationService projectApplicationService)
    {
        _projectApplicationService = projectApplicationService;
    }

    [McpServerTool, Description("Get teh first 10 applications from application list. Can optionally filter by recordstate.")]
    public async Task<List<List<string>>> GetSqlDataForUserPrompt(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("Filter by recordstate: active.")] string prompt,
            RecordState recordState = RecordState.Active
        )
    {
        var databaseType = config.GetValue<string>("DATABASETYPE");
        var databaseConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING");

        if (string.IsNullOrEmpty(databaseConnectionString))
        {
            throw new ArgumentException("DATABASECONNECTIONSTRING is not set in the configuration.");
        }
        if (string.IsNullOrEmpty(databaseType))
        {
            throw new ArgumentException("DATABASETYPE is not set in the configuration.");
        }

        var (records, totalCount) = await _projectApplicationService.GetProjectApplicationsAsync(
            1, // page
            10, // pageSize
            prompt // search
        );
         
        var result = new List<List<string>>();
        foreach (var record in records)
        {
            result.Add(new List<string>
            {
                record.Name,
                record.Descriptions,
                record.AppKey,
                record.DbVersion
            });
        }

        return result;
    }
     
}
