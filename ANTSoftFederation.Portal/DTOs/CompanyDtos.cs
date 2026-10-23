
namespace SorteFederation.ANTApplications.DTOs;

public class CreateCompanyDto : BaseCreateDto
{
    public required string Name { get; set; }
    public required string ContactPersonEmailAddress { get; set; }
    public bool? IsIntegrator { get; set; } = false;
}

public class UpdateCompanyDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public bool? IsIntegrator { get; set; }
}

public class CompanyDto : BaseDto
{
    public required string Name { get; set; }
    public bool? IsIntegrator { get; set; } = false;
}

public class CompanyListDto : BaseListDto
{
    public required string Name { get; set; }
    public bool? IsIntegrator { get; set; } = false;
    public DateTime CreatedOn { get; set; }
}