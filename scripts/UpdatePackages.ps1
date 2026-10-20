# UpdatePackages.ps1
param(
   [Parameter(Mandatory = $true)]
   [string]$Version
)

$ErrorActionPreference = "Stop"

$root = Split-Path $PSScriptRoot -Parent

$packages = @(
   @{ Project = (Join-Path $root "src/Application/Application.csproj"); Name = "ERP.Core.Application" },
   @{ Project = (Join-Path $root "src/Application/Application.csproj"); Name = "ERP.Core.Database.Application" },
   @{ Project = (Join-Path $root "src/Domain/Domain.csproj"); Name = "ERP.Core.Domain" },
   @{ Project = (Join-Path $root "src/Domain/Domain.csproj"); Name = "ERP.Core.Database.Domain" },
   @{ Project = (Join-Path $root "src/Infrastructure/Infrastructure.csproj"); Name = "ERP.Core.Infrastructure" },
   @{ Project = (Join-Path $root "src/Infrastructure/Infrastructure.csproj"); Name = "ERP.Core.Database.Infrastructure" },
   @{ Project = (Join-Path $root "test/ERP.Core.Manager.Api.Tests/ERP.Core.Manager.Api.ITests.csproj"); Name = "ERP.Core.Testing" }
)

foreach ($pkg in $packages) {
   Write-Host "Updating $($pkg.Name) -> $Version in $($pkg.Project)" -ForegroundColor Cyan
   dotnet add $pkg.Project package $pkg.Name --version $Version
}

Write-Host "Done." -ForegroundColor Green
