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

## Container image

The `Build and Publish Builders Image` GitHub Actions workflow builds the
Builders Dockerfile with the repository root as its context and pushes the image to GitHub Container Registry
when the Builders service or one of its shared project inputs changes. It tags
images with the branch, commit SHA, and `latest` on the default branch:

```text
ghcr.io/<github-owner>/antsoft-builders:<tag>
```

The workflow uses the repository's `GITHUB_TOKEN`; no registry password secret
is needed. New GHCR packages are private by default. Make the package public for
anonymous pulls, or configure an image-pull secret on private deployment
clusters. Aspire's local AppHost continues to run the Builders project from
source; deployment hosts can pull the published image above.
