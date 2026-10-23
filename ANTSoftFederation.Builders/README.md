# SorteFederation.Builders

The Builders service is the low-code runtime and metadata API. It uses the
Aspire-injected `ANTPortalDb` PostgreSQL connection for platform metadata and
resolves each tenant's PostgreSQL connection from the Portal tenant registry.

Required production settings:

- `ConnectionStrings__ANTPortalDb` (normally injected by Aspire)
- `Auth__Jwt__Key`, `Auth__Jwt__Issuer`, and `Auth__Jwt__Audience`
- `Encryption__Key` (a stable 16-, 24-, or 32-byte AES key; the committed Development key is for local use only)
- `Vault__Address` and `Vault__Token` when Vault-backed secrets are enabled

Optional AI settings:

- `AI__Provider`, `AI__Endpoint`, and `AI__Model`
- `AI__ApiKeyReference`, resolved through Vault/configuration/environment
- `AI__AuthScheme` (`bearer` or `api-key`)

The API is exposed under `/api/v1`. Health is available at `/health`; liveness
is exposed at `/alive` in development. The Kubernetes manifest in
`deploy/builders-deployment.yml` expects the production secrets to be created
outside the repository.
