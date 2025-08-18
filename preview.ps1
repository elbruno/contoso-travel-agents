#!/usr/bin/env pwsh
# This script builds, configures, and prepares the environment for running the AI Travel Agents applications on Windows.
# This script can be run directly via:
#   irm https://aka.ms/azure-ai-travel-agents-preview-win | pwsh

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

# Helper function to invoke setup hooks with platform detection
function Invoke-SetupHook {
    param(
        [string]$HookPath,
        [string]$HookName
    )
    
    $ps1Script = "$HookPath/setup.ps1"
    $shScript = "$HookPath/setup.sh"
    
    # Prefer PowerShell scripts on Windows, bash scripts elsewhere
    if ($IsWindows -or $env:OS -eq 'Windows_NT') {
        # On Windows, prefer .ps1 first
        if (Test-Path $ps1Script) {
            Write-Host ("{0}Running {1} setup using PowerShell script...{2}" -f $CYAN, $HookName, $NC)
            & pwsh -File $ps1Script | Out-Host
            return $LASTEXITCODE
        } elseif (Test-Path $shScript) {
            # Check if bash is available
            if (Get-Command bash -ErrorAction SilentlyContinue) {
                Write-Host ("{0}Running {1} setup using bash script (PowerShell script not found)...{2}" -f $CYAN, $HookName, $NC)
                bash $shScript | Out-Host
                return $LASTEXITCODE
            } else {
                Write-Host ("{0}{1}Neither PowerShell script nor bash found for {2} setup. Skipping.{3}" -f $RED, $CROSS, $HookName, $NC)
                return 1
            }
        } else {
            Write-Host ("{0}{1} setup script not found, skipping.{2}" -f $YELLOW, $HookName, $NC)
            return 0
        }
    } else {
        # On non-Windows, prefer .sh first
        if (Test-Path $shScript) {
            Write-Host ("{0}Running {1} setup using bash script...{2}" -f $CYAN, $HookName, $NC)
            bash $shScript | Out-Host
            return $LASTEXITCODE
        } elseif (Test-Path $ps1Script) {
            Write-Host ("{0}Running {1} setup using PowerShell script (bash script not found)...{2}" -f $CYAN, $HookName, $NC)
            & pwsh -File $ps1Script | Out-Host
            return $LASTEXITCODE
        } else {
            Write-Host ("{0}{1} setup script not found, skipping.{2}" -f $YELLOW, $HookName, $NC)
            return 0
        }
    }
}

# Step 0: Prerequisite checks
Write-Host ("{0}{1}Checking prerequisites...{2}" -f $BOLD, $BLUE, $NC)
$MISSING = 0

if (Get-Command node -ErrorAction SilentlyContinue) {
    $NODE_VERSION = node --version
    Write-Host ("{0}{1} Node.js version: {2}{3}" -f $GREEN, $CHECK, $NODE_VERSION, $NC)
} else {
    Write-Host ("{0}{1} Node.js is not installed. Please install Node.js (https://nodejs.org/){2}" -f $RED, $CROSS, $NC)
    $MISSING = 1
}

if (Get-Command npm -ErrorAction SilentlyContinue) {
    $NPM_VERSION = npm --version
    Write-Host ("{0}{1} npm version: {2}{3}" -f $GREEN, $CHECK, $NPM_VERSION, $NC)
} else {
    Write-Host ("{0}{1} npm is not installed. Please install npm (https://www.npmjs.com/){2}" -f $RED, $CROSS, $NC)
    $MISSING = 1
}

if (Get-Command docker -ErrorAction SilentlyContinue) {
    $DOCKER_VERSION = docker --version
    Write-Host ("{0}{1} Docker version: {2}{3}" -f $GREEN, $CHECK, $DOCKER_VERSION, $NC)
} else {
    Write-Host ("{0}{1} Docker is not installed. Please install Docker Desktop (https://www.docker.com/products/docker-desktop/){2}" -f $RED, $CROSS, $NC)
    $MISSING = 1
}

if ($MISSING -eq 1) {
    Write-Host ("{0}{1}One or more prerequisites are missing. Please install them and re-run this script.{2}" -f $RED, $BOLD, $NC)
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
$api_status = Invoke-SetupHook -HookPath "./infra/hooks/api" -HookName "API"
if ($api_status -ne 0) {
    Write-Host ("{0}{1}API setup failed with exit code $api_status. Exiting.{2}" -f $RED, $BOLD, $NC)
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
$ui_status = Invoke-SetupHook -HookPath "./infra/hooks/ui" -HookName "UI"
if ($ui_status -ne 0) {
    Write-Host ("{0}{1}UI setup failed with exit code $ui_status. Exiting.{2}" -f $RED, $BOLD, $NC)
    exit $ui_status
}

# Step 2.5: Create .env file for the UI
$uiEnvContent = @"
NG_API_URL=http://localhost:4000
"@
Set-Content -Path ./src/ui/.env -Value $uiEnvContent -Encoding UTF8
Write-Host ("{0}{1}.env file created in src/ui/.env.{2}" -f $GREEN, $BOLD, $NC)

# Step 3: Setup MCP tools (env, dependencies, docker build)
$mcp_status = Invoke-SetupHook -HookPath "./infra/hooks/mcp" -HookName "MCP tools"
if ($mcp_status -ne 0) {
    Write-Host ("{0}{1}MCP tools setup failed with exit code $mcp_status. Exiting.{2}" -f $RED, $BOLD, $NC)
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
