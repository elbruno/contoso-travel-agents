#!/usr/bin/env pwsh
# This script builds, configures, and prepares the environment for running the AI Travel Agents applications on Windows.
# This script can be run directly via:
#   irm https://aka.ms/azure-ai-travel-agents-preview-win | pwsh
#
# For ExecutionPolicy restrictions, run with:
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
        [string]$VersionFlag = '--version'
    )
    
    if (Get-Command $CommandName -ErrorAction SilentlyContinue) {
        try {
            $version = & $CommandName $VersionFlag 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host ("{0}{1} {2} version: {3}{4}" -f $GREEN, $CHECK, $DisplayName, $version, $NC)
            } else {
                Write-Host ("{0}{1} {2} found{3}" -f $GREEN, $CHECK, $DisplayName, $NC)
            }
        } catch {
            Write-Host ("{0}{1} {2} found{3}" -f $GREEN, $CHECK, $DisplayName, $NC)
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
        [string]$ComponentDisplayName
    )
    
    $psScript = "./infra/hooks/$Component/setup.ps1"
    $shScript = "./infra/hooks/$Component/setup.sh"
    
    if (Test-Path $psScript) {
        Write-Host ("{0}>> Running {1} setup (PowerShell)...{2}" -f $CYAN, $ComponentDisplayName, $NC)
        try {
            & $psScript
            $exitCode = $LASTEXITCODE
            if ($exitCode -ne 0) {
                if ($Component -eq "mcp") {
                    Write-Host ("{0}{1}{2} setup had issues (exit code $exitCode), but this won't stop the setup process.{3}" -f $YELLOW, $BOLD, $ComponentDisplayName, $NC)
                } else {
                    Write-Host ("{0}{1}{2} setup failed with exit code $exitCode. Exiting.{3}" -f $RED, $BOLD, $ComponentDisplayName, $NC)
                    exit $exitCode
                }
            }
        } catch {
            if ($Component -eq "mcp") {
                Write-Host ("{0}{1}{2} setup had issues: {3}. This won't stop the setup process.{4}" -f $YELLOW, $BOLD, $ComponentDisplayName, $_.Exception.Message, $NC)
            } else {
                Write-Host ("{0}{1}{2} setup failed: {3}. Exiting.{4}" -f $RED, $BOLD, $ComponentDisplayName, $_.Exception.Message, $NC)
                exit 1
            }
        }
    } elseif (Test-Path $shScript) {
        # Check if bash is available for .sh script
        if (Get-Command bash -ErrorAction SilentlyContinue) {
            Write-Host ("{0}>> Running {1} setup (bash)...{2}" -f $CYAN, $ComponentDisplayName, $NC)
            bash $shScript
            $exitCode = $LASTEXITCODE
            if ($exitCode -ne 0) {
                if ($Component -eq "mcp") {
                    Write-Host ("{0}{1}{2} setup had issues (exit code $exitCode), but this won't stop the setup process.{3}" -f $YELLOW, $BOLD, $ComponentDisplayName, $NC)
                } else {
                    Write-Host ("{0}{1}{2} setup failed with exit code $exitCode. Exiting.{3}" -f $RED, $BOLD, $ComponentDisplayName, $NC)
                    exit $exitCode
                }
            }
        } else {
            Write-Host ("{0}{1}Error: {2} setup requires bash, but bash is not available.{3}" -f $RED, $BOLD, $ComponentDisplayName, $NC)
            Write-Host ("{0}Please install Git Bash, WSL, or use a PowerShell setup script.{1}" -f $YELLOW, $NC)
            exit 1
        }
    } else {
        Write-Host ("{0}{1} setup script not found, skipping.{2}" -f $YELLOW, $ComponentDisplayName, $NC)
    }
}

# Step 0: Prerequisite checks
Write-Host ("{0}{1}Checking prerequisites...{2}" -f $BOLD, $BLUE, $NC)
$MISSING = 0

# Check git first since it's used for cloning
if (!(Test-And-ReportCommand "git" "Git" "https://git-scm.com/")) {
    $MISSING = 1
}

if (!(Test-And-ReportCommand "node" "Node.js" "https://nodejs.org/")) {
    $MISSING = 1
}

if (!(Test-And-ReportCommand "npm" "npm" "https://www.npmjs.com/")) {
    $MISSING = 1
}

if (!(Test-And-ReportCommand "docker" "Docker" "https://www.docker.com/products/docker-desktop/")) {
    $MISSING = 1
}

if ($MISSING -eq 1) {
    Write-Host ("{0}{1}One or more prerequisites are missing. Please install them and re-run this script.{2}" -f $RED, $BOLD, $NC)
    Write-Host ("{0}If you encounter ExecutionPolicy restrictions, run with:{1}" -f $YELLOW, $NC)
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
Invoke-Setup "api" "API"

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
Invoke-Setup "ui" "UI"

# Step 2.5: Create .env file for the UI
$uiEnvContent = @"
NG_API_URL=http://localhost:4000
"@
Set-Content -Path ./src/ui/.env -Value $uiEnvContent -Encoding UTF8
Write-Host ("{0}{1}.env file created in src/ui/.env.{2}" -f $GREEN, $BOLD, $NC)

# Step 3: Setup MCP tools (env, dependencies, docker build)
Invoke-Setup "mcp" "MCP tools"

# Step 4: Print next steps
Write-Host ("\n{0}{1}========================================{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}{1}Local environment is ready!{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}To run the API service, use:{1}" -f $BLUE, $NC)
Write-Host ("  {0}npm start --prefix ./src/api{1}" -f $BOLD, $NC)
Write-Host ("{0}To run the UI service, open a new terminal and use:{1}" -f $BLUE, $NC)
Write-Host ("  {0}npm start --prefix ./src/ui{1}" -f $BOLD, $NC)
Write-Host ("{0}{1}========================================{2}" -f $GREEN, $BOLD, $NC)
Write-Host ("{0}Tip: If you encounter ExecutionPolicy restrictions in the future, run:{1}" -f $YELLOW, $NC)
Write-Host ("  {0}pwsh -ExecutionPolicy Bypass -File preview.ps1{1}" -f $BOLD, $NC)
