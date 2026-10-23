# Rules metadata contract

Rules are tenant-scoped metadata records created through `POST /api/v1/rules` and evaluated through `POST /api/v1/rules/{ruleId}/evaluate`.

The `DefinitionJson` document supports:

```json
{
  "all": [
    { "field": "status", "operator": "eq", "value": "Approved" },
    { "any": [
      { "field": "amount", "operator": "gte", "value": 1000 },
      { "field": "priority", "operator": "eq", "value": "High" }
    ] }
  ],
  "actions": ["notify.manager", "start.approval-workflow"]
}
```

Supported operators are `eq`, `neq`, `contains`, `startswith`, `endswith`, `gt`, `gte`, `lt`, `lte`, `isNull`, and `isNotNull`. `all` and `any` may be nested. Evaluation returns `matched`, validation errors, and deduplicated declarative action names; action execution is intentionally handled by a separate worker boundary.

Rule changes require SystemAdmin, TenantAdmin, or Developer authorization and are protected by tenant ownership, optimistic version checks, JSON validation, size limits, and audit events.
