#!/usr/bin/env pwsh
# This script builds, configures, and prepares the environment for running the AI Travel Agents applications on Windows.
# This script can be run directly via:
#   irm https://aka.ms/azure-ai-travel-agents-preview-win | pwsh
#
# If you encounter ExecutionPolicy restrictions, run:
#   pwsh -ExecutionPolicy Bypass -File preview.ps1

$ErrorActionPreference = 'Stop'

# Colors (ANSI escape codes, supported in Windows 10+)
$RED    = "`e[0;31m"
$GREEN  = "`e[0;32m"
$YELLOW = "`e[1;33m"
$BLUE   = "`e[0;34m"
$CYAN   = "`e[0;36m"
$BOLD   = "`e[1m"
$NC     = "`e[0m" # No Color

# Unicode checkmark and cross
$CHECK = [char]0x2714  # ✔
$CROSS = [char]0x274C  # ❌

# Function to test and report command availability
function Test-And-ReportCommand {
    param(
        [string]$CommandName,
        [string]$DisplayName,
        [string]$InstallUrl,
        [switch]$GetVersion
    )
    
    if (Get-Command $CommandName -ErrorAction SilentlyContinue) {
        if ($GetVersion) {
            $version = & $CommandName --version
            Write-Host ("{0}{1} {2} version: {3}{4}" -f $GREEN, $CHECK, $DisplayName, $version, $NC)
        } else {
            Write-Host ("{0}{1} {2} is installed{3}" -f $GREEN, $CHECK, $DisplayName, $NC)
        }
        return $true
    } else {
        Write-Host ("{0}{1} {2} is not installed. Please install {2} ({3}){4}" -f $RED, $CROSS, $DisplayName, $InstallUrl, $NC)
        return $false
    }
}

# Function to invoke setup scripts with PowerShell preference
function Invoke-Setup {
    param(
        [string]$Component,
        [string]$ComponentPath
    )
    
    $psScript = Join-Path $ComponentPath "setup.ps1"
    $shScript = Join-Path $ComponentPath "setup.sh"
    
    if (Test-Path $psScript) {
        Write-Host ("{0}>> Running {1} setup (PowerShell)...{2}" -f $CYAN, $Component, $NC)
        try {
            & $psScript
            $exitCode = $LASTEXITCODE
            if ($null -eq $exitCode) { $exitCode = 0 }
            return $exitCode
        } catch {
            Write-Host ("{0}Error running {1} setup: {2}{3}" -f $RED, $Component, $_.Exception.Message, $NC)
            return 1
        }
    } elseif (Test-Path $shScript) {
        if (Get-Command bash -ErrorAction SilentlyContinue) {
            Write-Host ("{0}>> Running {1} setup (bash)...{2}" -f $CYAN, $Component, $NC)
            bash $shScript
            $exitCode = $LASTEXITCODE
            if ($null -eq $exitCode) { $exitCode = 0 }
            return $exitCode
        } else {
            Write-Host ("{0}{1}bash is not available and no PowerShell setup script found for {2}.{3}" -f $RED, $BOLD, $Component, $NC)
            Write-Host ("{0}Please install Git Bash or Windows Subsystem for Linux (WSL) to run .sh scripts.{1}" -f $YELLOW, $NC)
            Write-Host ("{0}Alternatively, you can install Git with bash tools from: https://git-scm.com/{1}" -f $YELLOW, $NC)
            return 1
        }
    } else {
        Write-Host ("{0}{1} setup script not found, skipping.{2}" -f $YELLOW, $Component, $NC)
        return 0
    }
}

# Step 0: Prerequisite checks
Write-Host ("{0}{1}Checking prerequisites...{2}" -f $BOLD, $BLUE, $NC)
$MISSING = 0

# Check git first since it's needed for cloning
if (-not (Test-And-ReportCommand -CommandName "git" -DisplayName "Git" -InstallUrl "https://git-scm.com/" -GetVersion)) {
    $MISSING = 1
}

# Check Node.js
if (-not (Test-And-ReportCommand -CommandName "node" -DisplayName "Node.js" -InstallUrl "https://nodejs.org/" -GetVersion)) {
    $MISSING = 1
}

# Check npm
if (-not (Test-And-ReportCommand -CommandName "npm" -DisplayName "npm" -InstallUrl "https://www.npmjs.com/" -GetVersion)) {
    $MISSING = 1
}

# Check Docker
if (-not (Test-And-ReportCommand -CommandName "docker" -DisplayName "Docker" -InstallUrl "https://www.docker.com/products/docker-desktop/" -GetVersion)) {
    $MISSING = 1
}

if ($MISSING -eq 1) {
    Write-Host ("{0}{1}One or more prerequisites are missing. Please install them and re-run this script.{2}" -f $RED, $BOLD, $NC)
    Write-Host ("{0}If you encounter ExecutionPolicy restrictions, run:{1}" -f $YELLOW, $NC)
    Write-Host ("  {0}pwsh -ExecutionPolicy Bypass -File preview.ps1{1}" -f $BOLD, $NC)
    exit 1
} else {
    Write-Host ("{0}All prerequisites are installed.{1}" -f $GREEN, $NC)
}

# Step 0: If not running inside the repo, clone it and re-run the script from there
$REPO_URL = "https://github.com/Azure-Samples/azure-ai-travel-agents.git"
$REPO_DIR = "azure-ai-travel-agents"
# Check for .git directory and preview.ps1 in the current directory
if (!(Test-Path .git) -or !(Test-Path preview.ps1)) {
    Write-Host ("{0}Cloning AI Travel Agents repository...{1}" -f $CYAN, $NC)
    git clone $REPO_URL
    Set-Location $REPO_DIR
    & pwsh preview.ps1 @args
    exit $LASTEXITCODE
}

# Step 1: Setup API dependencies
$api_status = Invoke-Setup -Component "API" -ComponentPath "./infra/hooks/api"
if ($api_status -ne 0) {
    Write-Host ("{0}{1}API setup failed with exit code {2}. Exiting.{3}" -f $RED, $BOLD, $api_status, $NC)
    exit $api_status
}

# Step 1.5: Create .env file for the user
if (!(Test-Path -Path ./src/api)) {
    New-Item -ItemType Directory -Path ./src/api | Out-Null
}
$envContent = @"
LLM_PROVIDER=docker-models
DOCKER_MODEL_ENDPOINT=http://localhost:12434/engines/llama.cpp/v1
DOCKER_MODEL=ai/phi4:14B-Q4_0

MCP_CUSTOMER_QUERY_URL=http://localhost:8080
MCP_DESTINATION_RECOMMENDATION_URL=http://localhost:5002
MCP_ITINERARY_PLANNING_URL=http://localhost:5003
MCP_CODE_EVALUATION_URL=http://localhost:5004
MCP_MODEL_INFERENCE_URL=http://localhost:5005
MCP_WEB_SEARCH_URL=http://localhost:5006
MCP_ECHO_PING_URL=http://localhost:5007
MCP_ECHO_PING_ACCESS_TOKEN=123-this-is-a-fake-token-please-use-a-token-provider
"@
Set-Content -Path ./src/api/.env -Value $envContent -Encoding UTF8
Write-Host ("{0}{1}.env file created in src/api/.env.{2}" -f $GREEN, $BOLD, $NC)

# Step 2: Setup UI dependencies
$ui_status = Invoke-Setup -Component "UI" -ComponentPath "./infra/hooks/ui"
if ($ui_status -ne 0) {
    Write-Host ("{0}{1}UI setup failed with exit code {2}. Exiting.{3}" -f $RED, $BOLD, $ui_status, $NC)
    exit $ui_status
}

# Step 2.5: Create .env file for the UI
$uiEnvContent = @"
NG_API_URL=http://localhost:4000
"@
Set-Content -Path ./src/ui/.env -Value $uiEnvContent -Encoding UTF8
Write-Host ("{0}{1}.env file created in src/ui/.env.{2}" -f $GREEN, $BOLD, $NC)

# Step 3: Setup MCP tools (env, dependencies, docker build)
$mcp_status = Invoke-Setup -Component "MCP tools" -ComponentPath "./infra/hooks/mcp"
if ($mcp_status -ne 0) {
    Write-Host ("{0}{1}MCP tools setup failed with exit code {2}. Exiting.{3}" -f $RED, $BOLD, $mcp_status, $NC)
    exit $mcp_status
}

# Step 4: Print next steps
Write-Host ("\n{0}{1}========================================{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}{1}Local environment is ready!{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}To run the API service, use:{1}" -f $BLUE, $NC)
Write-Host ("  {0}npm start --prefix ./src/api{1}" -f $BOLD, $NC)
Write-Host ("{0}To run the UI service, open a new terminal and use:{1}" -f $BLUE, $NC)
Write-Host ("  {0}npm start --prefix ./src/ui{1}" -f $BOLD, $NC)
Write-Host ("{0}{1}========================================{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}Note: If you encounter ExecutionPolicy issues in the future, run:{1}" -f $YELLOW, $NC)
Write-Host ("  {0}pwsh -ExecutionPolicy Bypass -File preview.ps1{1}" -f $BOLD, $NC)
