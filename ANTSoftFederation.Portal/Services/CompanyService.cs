 
using SorteFederation.Shared.Realtime;

namespace SorteFederation.ANTApplications.Services;

public class CompanyService : BaseService, ICompanyService
{
    private readonly ANTSoftPortalDbContext _context;
    private readonly IRealtimeEventBus _eventBus;

    public CompanyService(ANTSoftPortalDbContext context, IRealtimeEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetCompaniesAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Companies.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var companies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListDto
            {
               // CompanyId = c.Id,
                Name = c.Name,
                IsIntegrator = c.IsIntegrator,
                CreatedAt = c.CreatedAt, 
                UpdatedAt = c.UpdatedAt,
                IsActive = c.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (companies, totalCount);
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(Guid id)
    {
        var company = await _context.Companies
            .Where(c => c.Id == id)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                IsIntegrator = c.IsIntegrator,
                
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                IsActive = c.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return company;
    }

    public async Task<Guid> CreateCompanyAsync(CreateCompanyDto createDto)
    {
        var name = createDto.Name?.Trim();

        var contactEmail = createDto.ContactPersonEmailAddress?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(contactEmail) ||
            !new EmailAddressAttribute().IsValid(contactEmail))
            throw new InvalidOperationException("A company name and valid contact email address are required.");

        if (await _context.Users.AnyAsync(user => user.Username == contactEmail || user.Email == contactEmail))
            throw new InvalidOperationException("A user with this contact email address already exists.");

        var tenantAdminRole = await _context.Roles.SingleOrDefaultAsync(role => role.Name == RoleNames.TenantAdmin)
            ?? throw new InvalidOperationException("The Tenant Admin role must be seeded before creating a company.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var company = new Company
        {
            Name = name,
            IsIntegrator = createDto.IsIntegrator,
            ContactPersonEmailAddress = contactEmail,
            CreatedBy = createDto.CreatedBy
        };

        _context.Companies.Add(company);
        var tenantAdmin = new User
        {
            DisplayName = name,
            Username = contactEmail,
            Email = contactEmail,
            TelePhone = string.Empty,
            CompanyId = company.Id,
            EmailVerified = false,
            IsSystemAccount = false,
            ChangePasswordOnFirstLogin = true,
            CreatedBy = createDto.CreatedBy
        };
        _context.Users.Add(tenantAdmin);
        _context.UserRoles.Add(new UserRole { UserId = tenantAdmin.Id, RoleId = tenantAdminRole.Id });
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await PublishCompanyEventAsync(RabbitMQTopics.CompanyCreated, company.Id, company.Name);

        return company.Id;
    }

    public async Task<bool> UpdateCompanyAsync(Guid id, UpdateCompanyDto updateDto)
    {
        var company = await _context.Companies.FindAsync(id);
        
        if (company == null)
            return false;

        company.Name = updateDto.Name;
        company.IsIntegrator = updateDto.IsIntegrator;
        company.UpdatedBy = updateDto.UpdatedBy;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await PublishCompanyEventAsync(RabbitMQTopics.CompanyUpdated, company.Id, company.Name);
        return true;
    }

    public async Task<bool> DeleteCompanyAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        
        if (company == null)
            return false;

        company.RecordState = RecordState.Deleted;
        company.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        await PublishCompanyEventAsync(RabbitMQTopics.CompanyDeleted, company.Id, company.Name);
        return true;
    }

    private Task PublishCompanyEventAsync(string eventName, Guid companyId, string name) =>
        _eventBus.PublishAsync(new RealtimeEvent
        {
            TenantId = companyId,
            EventName = eventName,
            UserFacing = false,
            Data = System.Text.Json.JsonSerializer.SerializeToElement(new { companyId, name })
        });
}
