# CI/CD Pipeline — Polla Mundialista API

## Branch → Environment → App Service Mapping

| Branch | DevOps Environment | Azure App Service            | Region          |
|--------|--------------------|------------------------------|-----------------|
| `dev`  | `dev`              | `polla-mundialista-api-dev`  | Canada Central  |
| `qa`   | `qa`               | `polla-mundialista-api-qa`   | Canada Central  |
| `main` | `prod`             | `polla-mundialista-api`      | Canada Central  |

The pipeline is branch-driven: a push to any of the three branches triggers one build and one deploy. There is no multi-stage promotion or approval gate inside the YAML.

---

## One-Time Azure DevOps Setup

### 1. Service Connection

Create an Azure Resource Manager service connection named **`svc-conn-azure`**:

1. Project Settings → Service Connections → New service connection → Azure Resource Manager.
2. Select **Service Principal (automatic)** or use the credentials from the publish profiles.
3. Name it exactly `svc-conn-azure` — this matches the `azureSubscription` variable in the YAML.
4. Grant **Contributor** on the resource group that hosts all three App Services.

### 2. DevOps Environments

Create three environments (Pipelines → Environments → New):

| Name   | Purpose                    |
|--------|----------------------------|
| `dev`  | Development deployments     |
| `qa`   | QA / testing deployments    |
| `prod` | Production deployments      |

For `qa` and `prod` you can add **required approvals** (Environment → Approvals and checks → Add → Approvals) to gate deployments without changing the YAML.

### 3. Variable Group (secrets)

Create a variable group named **`polla-api-secrets`** (Pipelines → Library → + Variable group):

| Variable                  | Value                                      | Secret |
|---------------------------|--------------------------------------------|--------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string per env | Yes |
| `JwtSettings__Secret`     | JWT signing key per env                    | Yes    |

Link the variable group to each pipeline run in Library → Variable groups → Pipeline permissions.

Alternatively, store these as App Service **Application Settings** directly in the Azure portal — they override `appsettings.json` at runtime and never touch the pipeline.

---

## Promotion Workflow

```
feature/* → dev → qa → main
```

1. Developers push feature branches and open PRs targeting **`dev`**.
2. After QA sign-off, open a PR from `dev` → `qa`.
3. After QA approval, open a PR from `qa` → `main` for production release.

Each PR merge triggers the pipeline automatically for that target branch.

---

## Branch Policies (prevent direct pushes)

### `main` — production protection

In Project Settings → Repositories → Branches → `main` → Branch policies:

- **Require a minimum number of reviewers**: 1 (or 2 for stronger safety).
- **Check for linked work items**: optional but recommended.
- **Check for comment resolution**: enabled.
- **Limit merge types**: Squash merge or Rebase merge recommended.
- **Build validation**: add the pipeline as a required build before merge.

### `qa` — QA gate

Same policy as `main` but at least 1 reviewer. Add the pipeline as a build validation so tests must pass before merging.

### `dev` — optional

You may leave `dev` open for direct pushes by the development team, or add a light policy (build validation only, no approval required).

---

## Secrets — What Goes Where

| Secret                       | Storage                                      |
|------------------------------|----------------------------------------------|
| Connection strings           | Azure App Service → Configuration → App Settings |
| JWT signing key              | Azure App Service → Configuration → App Settings |
| Publish credentials          | Never in YAML — use the service connection   |

The YAML contains **no secrets**. All sensitive values are injected by the App Service runtime or the DevOps variable group at deploy time.

---

## App Service URLs

| Environment | URL |
|-------------|-----|
| Dev  | https://polla-mundialista-api-dev-cgeee4c9b4fhbba4.canadacentral-01.azurewebsites.net |
| QA   | https://polla-mundialista-api-qa-a2dhb3b3erdvabc7.canadacentral-01.azurewebsites.net  |
| Prod | https://polla-mundialista-api-aaaubkg9f8g6ekgu.canadacentral-01.azurewebsites.net     |
