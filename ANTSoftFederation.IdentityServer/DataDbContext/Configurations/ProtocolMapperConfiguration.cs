using Microsoft.EntityFrameworkCore;

namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;

internal class ProtocolMapperConfiguration : IEntityTypeConfiguration<ProtocolMapper>
{
    public void Configure(EntityTypeBuilder<ProtocolMapper> entity)
    {
        entity.HasIndex(p => new { p.ClientId, p.Name }).IsUnique(); //.HasFilter("ClientId IS NOT NULL");
        entity.HasIndex(p => new { p.ClientScopeId, p.Name }).IsUnique(); //.HasFilter("ClientScopeId IS NOT NULL");
    }
}