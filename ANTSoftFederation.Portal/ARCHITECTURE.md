# ANTSoft Portal Addon Architecture

The Portal is the Angular shell. Internal shared libraries are compiled into the shell and use the workspace's single Angular and Tailwind installation.

Addons are described by `AddonManifest` version `1.0`. The shell loads manifests from `GET /api/v1/addons/manifests`, contributes permitted menu links beneath `custom`, and creates one route per contribution. The shell passes `config` and `AddonContext` as DOM properties to the custom element.

Addons obtain authentication only through `context.auth.getToken()` and must emit `auth:unauthorized` for a 401. They must not read shell storage or redirect the browser.

Only trusted addon scripts may run as native custom elements. Untrusted addons should use `loadMode: "iframe"`; the host applies a sandbox and does not expose shell storage to the frame. Production deployments must restrict addon script origins with CSP.
