# Designer Studio template package API

This API is the independent catalog and release boundary for community-authored starter templates. It uses the Portal-issued JWT, its own PostgreSQL database, and immutable `.anttemplate` package files. Builder's existing database templates are not modified by this service.

## Package format v1

An `.anttemplate` is a ZIP archive containing a root-level `manifest.json` and the template's page/component definitions and assets. The manifest identifies the package and declares its own data models; it does not refer to a customer's tables.

```json
{
  "formatVersion": 1,
  "template": {
    "key": "purchase-operations",
    "name": "Purchase Operations",
    "version": "1.0.0"
  },
  "dataModels": [
    {
      "key": "purchaseRequest",
      "name": "Purchase Request",
      "fields": [
        { "key": "requestNumber", "type": "string" },
        { "key": "requestedBy", "type": "string" },
        { "key": "requestedAt", "type": "date" }
      ]
    }
  ],
  "bindings": [
    { "pageKey": "request-form", "componentKey": "request-number", "modelKey": "purchaseRequest", "fieldKey": "requestNumber" }
  ]
}
```

Each binding's `modelKey` and `fieldKey` must resolve to a model and field declared in the same package. The package archive is stored as an opaque immutable file; the API validates its ZIP paths, size, manifest identity, model definitions, and binding references without extracting untrusted entries.

## Workflow

Only accounts assigned the `Designer` or `SystemAdmin` Portal role can enter the Design Studio or call template-authoring APIs. The API verifies these roles from the Portal-issued JWT; hiding the UI is not treated as an authorization boundary. Release review and publish/reject actions remain restricted to `SystemAdmin`.

1. An authorized author creates a template record under `/api/v1/studio/templates` and saves the working canvas definition under `/api/v1/studio/templates/{id}/draft`.
2. The author uploads a new semantic version under `/api/v1/studio/templates/{id}/releases`.
3. The author submits a draft release for review.
4. A `SystemAdmin` lists pending reviews at `/api/v1/review/releases`, then publishes or rejects a submitted release. Rejections include reviewer feedback that the author can see in their release list.
5. Authenticated Builder clients browse and download published versions under `/api/v1/templates`; this read-only catalog remains available to normal authenticated Builder users and does not grant authoring access.

Published release records and package files are immutable. A change is a new release version.


Figma Token = figd_L8CBLQpFZw2p04T7kubpeGBiJYt5oPjjCuXr20-7