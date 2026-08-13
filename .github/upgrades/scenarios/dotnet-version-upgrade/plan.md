# Upgrade RazdanehCMS to .NET 10

## Understanding
The user asked to run an automated assessment of the RazdanehCMS solution and generate an upgrade plan to move projects from .NET 6 to .NET 10 (target: net10.0). I will use the assessment output to produce a prioritized plan that minimizes risk for the Razor Pages web project and related class libraries.

## Assumptions
- The assessment has been generated for the full solution and saved to assessment.md.
- The repository is on a working branch named `upgrade-dotnet-10-1` (recorded in scenario-instructions.md). The agent could not create the branch locally because git wasn't available in the agent environment.
- Flow Mode: Automatic (agent will proceed unless blocked).
- All projects currently target net6.0 and will be migrated to net10.0 unless otherwise noted.

## Approach
1. Use the generated assessment to identify blocking issues (incompatible NuGet packages, API breaking changes, security vulnerabilities) and mark them as high priority.
2. Update project TargetFramework entries to net10.0 in an isolated, incremental manner: migrate library projects first, then infrastructure/persistence, then web projects (Razor Pages prioritized), fixing compile errors after each project group.
3. Upgrade NuGet packages to compatible versions, preferring latest stable releases that support net10.0; when incompatible, document replacements or workarounds.
4. Build and run tests after each group; fix warnings and treat them as errors.
5. Update CI/CD and Docker base images at the end and run final end-to-end verification.

## Key Files
- RazdanehCMS.sln — solution to upgrade
- .github/upgrades/scenarios/dotnet-version-upgrade/assessment.md — automated assessment (generated)
- .github/upgrades/scenarios/dotnet-version-upgrade/plan.md — this plan (written)
- scenario-instructions.md — contains confirmed parameters and branch info
- Typical project files to modify: **CMS/**/**/**/*.csproj (TargetFramework changes)

## Risks & Open Questions
- Some NuGet packages reported as incompatible or deprecated and may have no direct net10.0 replacement (NuGet.0001, NuGet.0005). These will be documented as blocking items.
- Binary-incompatible APIs (Api.0001) will require code changes; timeline depends on complexity.
- Git was not available to the agent; the user must ensure the working branch exists locally or run the branch creation commands suggested by the agent.

## Steps
1. step-1: Validate assessment artifact — open assessment.md and extract a short list of blocking issues (NuGet incompatibilities, API breaking changes) for the top-level projects.
2. step-2: Create upgrade plan.md — produce a project-by-project upgrade sequence and task list (libraries → infrastructure → persistence → web) and write plan.md to the scenario folder.
3. step-3: Prepare project update patch (analysis pass) — for each project group, list exact csproj changes required (TargetFramework) and required package version updates.
4. step-4: Apply TargetFramework updates for library projects — update csproj TargetFramework to net10.0 for core libraries, build, and fix compile issues.
5. step-5: Apply package upgrades for library projects — update NuGet packages to compatible versions and rebuild until green.
6. step-6: Repeat steps 4-5 for infrastructure/persistence projects.
7. step-7: Update Razor Pages web project(s) — change TargetFramework to net10.0, update ASP.NET packages, run app locally to smoke test pages and routing.
8. step-8: Update CI/CD, Dockerfiles, and environment settings (runtime images to .NET 10) and run full-solution build and tests.
9. step-9: Create progress-details.md for each completed step and call complete_task per task (handled during execution stage).


# End of plan
