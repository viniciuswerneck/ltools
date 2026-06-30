param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "dist",
    [string]$Version = "0.4.0",
    [switch]$SkipZip
)

$ErrorActionPreference = "Stop"
$RootDir = $PSScriptRoot
$PublishDir = Join-Path $RootDir $OutputDir

Write-Host "=== LTools Publish Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration"
Write-Host "Runtime: $Runtime"
Write-Host "Output: $PublishDir"
Write-Host ""

# 1. Clean previous output
if (Test-Path $PublishDir) {
    Write-Host "Cleaning previous output..." -ForegroundColor Yellow
    Remove-Item -Path $PublishDir -Recurse -Force
}

# 2. Restore and build all plugins
Write-Host "`n=== Building Plugins ===" -ForegroundColor Cyan
$pluginProjects = Get-ChildItem -Path (Join-Path $RootDir "plugins") -Recurse -Filter "*.csproj"
foreach ($proj in $pluginProjects) {
    Write-Host "Building plugin: $($proj.Name)..." -ForegroundColor Green
    dotnet build $proj.FullName -c $Configuration --no-restore -p:RestorePackagesPath=$env:NUGET_PACKAGES 2>$null
    if ($LASTEXITCODE -ne 0) {
        dotnet build $proj.FullName -c $Configuration
        if ($LASTEXITCODE -ne 0) { throw "Plugin build failed: $($proj.Name)" }
    }
}

# 3. Build LTools.Core (dependency)
Write-Host "`n=== Building Core ===" -ForegroundColor Cyan
$coreProj = Join-Path (Join-Path $RootDir "src\LTools.Core") "LTools.Core.csproj"
dotnet build $coreProj -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Core build failed" }

# 4. Publish LTools.UI self-contained single-file
Write-Host "`n=== Publishing LTools.UI ===" -ForegroundColor Cyan
$uiProj = Join-Path (Join-Path $RootDir "src\LTools.UI") "LTools.UI.csproj"
dotnet publish $uiProj -c $Configuration -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $PublishDir
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# 5. Copy plugin DLLs and dependencies to publish output
Write-Host "`n=== Copying Plugins ===" -ForegroundColor Cyan
$pluginsSource = Join-Path $RootDir "plugins"
$pluginsDest = Join-Path $PublishDir "plugins"
New-Item -ItemType Directory -Path $pluginsDest -Force | Out-Null

$allPluginFiles = Get-ChildItem -Path $pluginsSource -Include "*.dll", "*.deps.json", "*.xml", "*.pdb" -File

foreach ($file in $allPluginFiles) {
    Copy-Item $file.FullName -Destination (Join-Path $pluginsDest $file.Name) -Force
    Write-Host "  Copied: $($file.Name)"
}

# Copy only Windows runtimes (needed by MySqlConnector on some configs)
$runtimesSource = Join-Path $pluginsSource "runtimes"
if (Test-Path $runtimesSource) {
    $winDirs = @("win-x64", "win-arm64", "win-x86")
    foreach ($dir in $winDirs) {
        $src = Join-Path $runtimesSource $dir
        if (Test-Path $src) {
            $dst = Join-Path $pluginsDest "runtimes"
            Copy-Item $src -Destination (Join-Path $dst $dir) -Recurse -Force
            Write-Host "  Copied: runtimes/$dir/"
        }
    }
}

# 6. Remove unnecessary files from publish output
Write-Host "`n=== Cleaning Publish Output ===" -ForegroundColor Yellow
$removePatterns = @("*.pdb", "*.xml", "*.deps.json")
foreach ($pattern in $removePatterns) {
    Get-ChildItem -Path $PublishDir -Filter $pattern -Recurse | Where-Object {
        $_.FullName -notlike "*\plugins\*"
    } | Remove-Item -Force
}

# 7. Create ZIP
if (-not $SkipZip) {
    Write-Host "`n=== Creating ZIP ===" -ForegroundColor Cyan
    $zipName = "LTools-$Runtime-$Version.zip"
    $zipPath = Join-Path $RootDir $zipName
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($PublishDir, $zipPath)
    Write-Host "ZIP package: $zipPath"
}

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "Output directory: $PublishDir"
