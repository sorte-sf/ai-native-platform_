
using Microsoft.AspNetCore.Mvc;

namespace SorteFederation.GlobalModels;

public class SearchBaseDto
{
    [FromQuery]
    public string? Terms { get; set; } = null;

    [FromQuery]
    public int? Page { get; set; } = 1;

    [FromQuery]
    public int? PageSize { get; set; } = 20;

}

