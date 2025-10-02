---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the new per-unit directory structure.
---

You are an intelligent developer validating this .NET Aspire workshop. You will step through modules 1–9 only, using the documentation under the `workshop/` folder and the `start` and `complete` solutions. You will test code builds, run integration tests, and review the docs for clarity and flow. If any requirements are unclear, you will ask clarifying questions and document assumptions.

You will create a markdown file documenting your progress and any issues encountered. Include a section for recommended documentation improvements for any modules that were unclear or required assumptions.

Scope note (important): This prompt intentionally excludes modules 10–15, including GitHub Models integration and Docker integration. Do not test or modify AI provider setup, MCP servers, or GitHub Models. Only validate modules 1–9.

## Purposes of this testing procedure

There are three main objectives:

1. Validate modules 1–9 independently using the provided docs and code.
2. Identify and document any issues or challenges encountered during the workshop.
3. Verify that the complete solution builds, tests pass, and deployment guidance (module 9) is actionable.

## Workshop Structure (modules 1–9)

The workshop is organized as 15 modules; this testing procedure focuses strictly on the first nine modules:

1. Setup & Installation (`workshop/Lesson-01-Setup/README.md`)
2. Service Defaults (`workshop/Lesson-02-ServiceDefaults/README.md`)
3. Developer Dashboard & Orchestration (`workshop/Lesson-03-Dashboard-AppHost/README.md`)
4. Service Discovery (`workshop/Lesson-04-ServiceDiscovery/README.md`)
5. Integrations (`workshop/Lesson-05-Integrations/README.md`)
6. Telemetry (`workshop/Lesson-06-Telemetry/README.md`)
7. Database (`workshop/Lesson-07-Database/README.md`)
8. Integration Testing (`workshop/Lesson-08-Integration-Testing/README.md`)
9. Deployment (`workshop/Lesson-09-Deployment/README.md`)

Supporting code snapshots:

- `start/` — starting project used during the workshop
- `complete/` — a completed reference implementation (includes `IntegrationTests/`)

## Environment and Credentials

- Modules 1–8 require no secrets or external credentials.
- Module 9 (Deployment) requires an Azure subscription and the Azure Developer CLI (`azd`). Authentication and parameter collection are typically handled interactively by `azd`. Environment variables are optional; you can pass subscription and location on the command line when running `azd`.

Optional convenience environment variables (Windows PowerShell):

```powershell
# Optional, for non-interactive provisioning in Module 9
$env:AZURE_SUBSCRIPTION_ID = "<your-subscription-id>"
$env:AZURE_LOCATION = "<your-azure-region>"  # e.g. eastus, westus3
```

## Important Notes for Testing (modules 1–9)

- Use the per-lesson `code/` folders as the working context for modules 1–9; consult `complete/` as a known-good reference. The policy is:
  - Lesson 1 contains no code changes and does not require a `code/` copy.
  - For Lesson 2, copy the entire contents of `start/` into `workshop/Lesson-02-ServiceDefaults/code/`, ensuring that all files in `Api/Properties/` (including `launchSettings.json`) are copied exactly, to prevent port drift and configuration mismatches. Then perform the steps described in `workshop/Lesson-02-ServiceDefaults/README.md` against that copy.
  - For each subsequent lesson (Lesson N where N >= 3), copy the contents of `workshop/Lesson-(N-1)-*/code/` into `workshop/Lesson-N-*/code/`, ensuring that all files in `Api/Properties/` (including `launchSettings.json`) are copied exactly, to prevent port drift and configuration mismatches. Then perform the steps described in `workshop/Lesson-N-*/README.md` against that copy.
  - NEVER copy the `.vs` folder (IDE state) between lessons. If a `.vs` directory appears inside any `workshop/Lesson-XX-*/code` folder, delete it immediately: `Remove-Item -Recurse -Force .\workshop\Lesson-XX-*/code/.vs`.
  - Prefer excluding transient build/test artifacts (`bin/`, `obj/`, `TestResults/`) unless a lesson explicitly requires inspecting them. This prevents locked file copy errors and keeps snapshots minimal.
  - At the end of each lesson run and verify a successful build for that lesson's `code/` folder.
  - Ensure the `complete/` solution builds cleanly and all integration tests pass.
- For deployment (module 9):
  - Run `azd` commands from the correct directory (where `azure.yaml` resides or will be generated).
  - If `azure.yaml` is not present yet, run `azd init` first (as described in the docs) and follow the prompts.
  - Confirm the web project is exposed with external endpoints (look for `.WithExternalHttpEndpoints()` in `complete/AppHost/Program.cs`).

## Known Checks and Common Pitfalls (relevant to this repo)

- Directory context for `azd`: Provisioning often fails if run from the wrong directory. Confirm you are in the app root (for this repo, typically `complete/`) or use `--cwd`.
- External HTTP endpoints: The web app should call `.WithExternalHttpEndpoints()` in AppHost to be reachable post-deployment.
- NuGet restore/build: If encountering restore issues, run a clean build sequence: `dotnet clean`, `dotnet restore`, then `dotnet build`.

## Procedure

- **Preparation (Module 1 – Setup & Installation)**
  - Follow `workshop/Lesson-01-Setup/README.md` to install prerequisites (including .NET SDK 9, `azd` for Module 9, and any recommended tooling).
  - Verify the repository builds locally using the commands below.

- **Modules 2–8**
  - Workflow (per-lesson `code/` folders):
    1. Lesson 1 requires no code changes and has no `code/` work.
    2. For Lesson 2: copy `start/` → `workshop/Lesson-02-ServiceDefaults/code/` and apply all steps in `workshop/Lesson-02-ServiceDefaults/README.md` to that copy.
    3. For each Lesson N (N ≥ 3): copy `workshop/Lesson-(N-1)-*/code/` → `workshop/Lesson-N-*/code/` and apply all steps in `workshop/Lesson-N-*/README.md` to that copy.
    4. After completing the steps for the lesson, run a build in the lesson's `code/` folder and confirm it succeeds. Document any deviations required to make the build succeed.
  - Use `complete/` as a reference if you get blocked; document the differences between the lesson `code/` folder and `complete/`.

- **Module 9 – Deployment**
  - Follow `workshop/Lesson-09-Deployment/README.md`.
  - If `azure.yaml` is not present, initialize with `azd init` and confirm it detects the AppHost correctly. Then provision/deploy as appropriate.
  - If you cannot deploy to Azure in your environment, document the exact step where you stop and why (e.g., lack of subscription access), but still validate that `azd init` runs and generates expected files.

## Evaluation

- At the end of each module, verify that the described functionality is present and works as intended (builds succeed, pages load, services communicate, etc.).
- For the `complete/` solution, ensure that all integration tests pass and that AppHost defines external HTTP endpoints for the web project.
- Document any issues, missing prerequisites, or unclear steps, and propose concise improvements to the corresponding workshop module docs.
- Note any assumptions you made to progress, especially where the docs could be clarified.

## Reproducible Commands (Windows PowerShell)

Build and quick verification:

```powershell
# Build start solution
cd start
dotnet build .\MyWeatherHub.sln --verbosity minimal
cd ..

# Build complete solution
cd complete
dotnet build .\MyWeatherHub.sln --verbosity minimal
cd ..
```

Run integration tests (complete solution):

```powershell
cd complete
# Run all tests (includes IntegrationTests)
dotnet test .\MyWeatherHub.sln --verbosity minimal
cd ..
```

## Working in lesson `code/` folders (copy + build examples)

Use these PowerShell commands to create the per-lesson working copies and build from them. They assume you are running PowerShell from the repository root.

```powershell
# Lesson 1: no code changes required

# Lesson 2: copy `start` into Lesson-02 `code` folder
# Safety: remove IDE and test artifacts in `start` before copying. NEVER copy `.vs`.
if (Test-Path .\start\.vs) { Remove-Item -Recurse -Force .\start\.vs -ErrorAction SilentlyContinue }
if (Test-Path .\start\TestResults) { Remove-Item -Recurse -Force .\start\TestResults -ErrorAction SilentlyContinue }
if (Test-Path .\workshop\Lesson-02-ServiceDefaults\code) { Remove-Item -Recurse -Force .\workshop\Lesson-02-ServiceDefaults\code }
Get-ChildItem .\start -Force | Where-Object { $_.Name -notin '.vs' } | ForEach-Object { Copy-Item -Recurse -Force $_.FullName (Join-Path .\workshop\Lesson-02-ServiceDefaults\code $_.Name) }

# Build Lesson-02 code
Push-Location .\workshop\Lesson-02-ServiceDefaults\code
dotnet build .\MyWeatherHub.sln --verbosity minimal
Pop-Location

# Lesson N -> Lesson N+1: copy previous lesson's code into next lesson (exclude .vs & transient artifacts)
# Example: copy Lesson-02 code into Lesson-03
if (Test-Path .\workshop\Lesson-02-ServiceDefaults\code\.vs) { Remove-Item -Recurse -Force .\workshop\Lesson-02-ServiceDefaults\code\.vs -ErrorAction SilentlyContinue }
if (Test-Path .\workshop\Lesson-02-ServiceDefaults\code\TestResults) { Remove-Item -Recurse -Force .\workshop\Lesson-02-ServiceDefaults\code\TestResults -ErrorAction SilentlyContinue }
if (Test-Path .\workshop\Lesson-03-Dashboard-AppHost\code) { Remove-Item -Recurse -Force .\workshop\Lesson-03-Dashboard-AppHost\code }
Get-ChildItem .\workshop\Lesson-02-ServiceDefaults\code -Force | Where-Object { $_.Name -notin '.vs' } | ForEach-Object { Copy-Item -Recurse -Force $_.FullName (Join-Path .\workshop\Lesson-03-Dashboard-AppHost\code $_.Name) }

# Optional robust alternative (commented):
# robocopy .\workshop\Lesson-02-ServiceDefaults\code .\workshop\Lesson-03-Dashboard-AppHost\code /MIR /XD .vs TestResults bin obj /NFL /NDL /NJH /NJS /NP | Out-Null

# Build Lesson-03 code
Push-Location .\workshop\Lesson-03-Dashboard-AppHost\code
dotnet build .\MyWeatherHub.sln --verbosity minimal
Pop-Location
```

Sanity checks in AppHost (complete solution):

```powershell
# Confirm external HTTP endpoints are enabled for the web app
Select-String -Path "complete\AppHost\Program.cs" -Pattern "WithExternalHttpEndpoints"
```

Deployment (Module 9) — optional if you have Azure access:

```powershell
cd complete

# Ensure Azure Developer CLI is installed and up to date (optional)
winget upgrade Microsoft.Azd

# Initialize for deployment (if azure.yaml doesn't exist yet)
azd init
# Follow prompts: Use code in current directory -> Confirm -> Choose an environment name

# Provision (use explicit parameters or interactive prompts)
# Option A: Interactive
azd provision

# Option B: With explicit parameters
azd provision --subscription "$env:AZURE_SUBSCRIPTION_ID" --location "$env:AZURE_LOCATION"

# Deploy the application
azd deploy

# Show the deployed endpoints and resource info
azd show

# Optional cleanup
# azd down --purge --force
cd ..
```

## Deliverable

- Produce a markdown report (e.g., `TESTING-NOTES.md`) capturing:
  - Module-by-module status (1–9)
  - Build/test results and any failures encountered
  - Exact steps/commands you ran when deviating from docs
  - Recommended documentation improvements (concise, actionable)
  - Any environmental constraints or assumptions

## Out-of-scope for this prompt

- GitHub Models integration (workshop module 14)
- Docker integration (module 15)
- Any MCP server creation/publishing workflows
- AI chat templates, vector stores, Qdrant, or related JavaScript assets

Focus exclusively on modules 1–9 in this repository.

## Open questions and issues from modules 1–9

### Low-risk fixes (apply now)

- [x] Module 3 (Dashboard & App Host): The instruction says "Set Default Project"; in Visual Studio the label is "Set as Startup Project". Confirmed and updated wording.
- [x] Module 4 (Typos): "MyWeatheApp" appears in text; should be "MyWeatherHub". Fixed.
- [x] Module 5 (Redis admin UI naming): The text references "Redis Commander", but the instructions and screenshots refer to "Redis Insight". Standardized on "Redis Insight".
- [x] Module 5 (Output caching guidance): Clarified that removing the default in-memory policy avoids duplication; policies/tags still apply with Redis as the store when using `builder.AddRedisOutputCache("cache")`.
- [x] Module 5 (Container runtime prerequisite timing): The Redis container requires Docker/Podman. Added an explicit reminder to start Docker/Podman before launching the AppHost.
- [x] Module 6 (Telemetry duplication): Added a note that ServiceDefaults provides baseline OTEL and custom meters/sources are additive.
- [x] Module 7 (Persistent containers): Clarified that `WithLifetime(ContainerLifetime.Persistent)` and `WithInitFiles(...)` are optional enhancements.
- [x] Module 7 (EF usage in Blazor): The favorites feature compares `Zone` records with `FavoriteZones.Contains(context)`. Because `Zone` is a record, value equality applies, which is intended. Added a brief note to reduce confusion.
- [x] Module 8 (Package versions): The doc shows MSTest 3.8.2; `complete/IntegrationTests` uses 3.9.3. Added a note that any recent 3.x is fine.
- [x] Module 9 (Prereqs): Explicitly mentioned `azd login` and subscription selection; noted required subscription permissions.
- [x] Module 9 (Working directory): Reiterated to run `azd` from the directory containing the AppHost (typically `complete/`).
- [x] Cross-cutting (Health endpoints visibility): Added a note that `MapDefaultEndpoints()` only maps `/health` and `/alive` in Development.

### Recommended improvements

- [ ] Module 3 (VS Code): Provided `launch.json` snippet runs the AppHost; confirm it's included in the repo for `start/` (we only have compound configs for running Api and Web). Consider adding the AppHost launch snippet to `start/.vscode/launch.json` or clarify manual creation.
- [x] Module 3 (Error simulation): Docs mention an error after clicking ~5 different cities. Added a brief note explaining `NwsManager` simulates an exception roughly every 5th request.
- [x] Module 4 (Service discovery URL consistency): The sample changes for `NwsManager` now use `new Uri("https://weather-api")` (no trailing slash) and a `zoneUrl` with a leading slash, matching the complete code.
- [ ] Module 4 (External service modeling): Add a short note that this is development-time modeling and doesn't proxy traffic—requests still go to the actual external service.
- [x] Module 4 (External service modeling): Added a short note that this is development-time modeling and doesn't proxy traffic—requests still go to the actual external service.
- [x] Module 9 (Service names): Updated examples to use `myweatherhub`/`api` instead of `webfrontend`/`apiservice`.
- [ ] Cross-cutting (IT-Tools container): The AppHost includes an extra `it-tools` container with external endpoint. Either remove it from `complete` for focus or add a brief note so users aren't surprised by the extra endpoint.
- [x] Cross-cutting (IT-Tools container): Added a brief note that an optional `it-tools` container may appear and can be ignored for modules 1–9.

### Open questions (need answers before proceeding)

- [ ] Cross-cutting (AI dependency present in complete): The `complete` solution wires an AI chat client:
  - `complete/AppHost/Program.cs` defines `builder.AddGitHubModel("chat-model", "gpt-4o-mini")`.
  - `complete/MyWeatherHub/Program.cs` configures `AddAzureChatCompletionsClient("chat-model").AddChatClient()`.
  - `complete/MyWeatherHub/ForecastSummarizer.cs` uses `IChatClient` to summarize text.
  - `complete/MyWeatherHub/Components/Pages/Home.razor` calls the summarizer after retrieving a forecast.
  This likely requires credentials (GitHub Models or Azure AI) and conflicts with the statement that modules 1–8 require no secrets. Without credentials, selecting a zone will attempt to call the model and can fail. Proposals:
  - Add a guard/fallback in `ForecastSummarizer` (and/or caller) to gracefully skip summarization if the chat client isn't configured, or
  - Document optional environment variable setup for Module 14 (out of scope here) and instruct testers to avoid the summarization path when validating modules 1–9, or
  - Provide a feature flag to disable summarization for modules 1–9.
- [ ] Module 8 (Empty test file): `complete/IntegrationTests/WeatherBackgroundTests.cs` exists but is empty. Decide whether to remove it or provide a minimal test to avoid confusion.
