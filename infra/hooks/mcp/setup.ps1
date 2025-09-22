# This script builds and sets up the MCP containers for Windows PowerShell.

##########################################################################
# MCP Tools
##########################################################################
Write-Host '>> Creating .env file for the MCP servers...'

# Get all tools that have .env.sample files
$allTools = @('echo-ping', 'customer-query', 'destination-recommendation', 'itinerary-planning', 'code-evaluation', 'model-inference', 'web-search')
$toolsWithEnv = @()

foreach ($tool in $allTools) {
    $envSample = "./src/tools/$tool/.env.sample"
    $envFile = "./src/tools/$tool/.env"
    $envDockerFile = "./src/tools/$tool/.env.docker"
    if (Test-Path $envSample) {
        Write-Host "Creating .env file for $tool..."
        $toolsWithEnv += $tool
        if (-not (Test-Path $envFile)) {
            Copy-Item $envSample $envFile
            Add-Content $envFile "# File automatically generated on $(Get-Date)"
            Add-Content $envFile "# See .env.sample for more information"
        }
        if (-not (Test-Path $envDockerFile)) {
            Copy-Item $envSample $envDockerFile
            Add-Content $envDockerFile "# File automatically generated on $(Get-Date)"
            Add-Content $envDockerFile "# See .env.sample for more information"
        }
        # Install dependencies for the tool service
        Write-Host ">> Installing dependencies for $tool service..."
        $nodeModules = "./src/tools/$tool/node_modules"
        if (-not (Test-Path $nodeModules)) {
            npm ci --prefix=./src/tools/$tool
            if ($LASTEXITCODE -ne 0) {
                Write-Host "$tool dependencies installation failed with exit code $LASTEXITCODE. Exiting."
                exit $LASTEXITCODE
            }
        } else {
            Write-Host "Dependencies for $tool service already installed."
        }
    } else {
        Write-Host "No .env.sample found for $tool, skipping..."
    }
}

# Enable Docker Desktop Model Runner (only if Docker Desktop is available)
Write-Host 'Enabling Docker Desktop Model Runner...'
try {
    $dockerVersionOutput = docker version --format json 2>$null | ConvertFrom-Json
    if ($dockerVersionOutput.Server.Platform.Name -like "*Docker Desktop*") {
        docker desktop enable model-runner --tcp 12434 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host 'Docker Desktop Model Runner enabled successfully.'
        } else {
            Write-Host 'Warning: Failed to enable Docker Desktop Model Runner.'
        }
        
        docker model pull ai/phi4:14B-Q4_0 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host 'Docker model pulled successfully.'
        } else {
            Write-Host 'Warning: Failed to pull Docker model.'
        }
    } else {
        Write-Host 'Skipping Docker Desktop specific commands (not running Docker Desktop).'
    }
} catch {
    Write-Host 'Warning: Could not determine Docker version. Skipping Docker Desktop specific commands.'
}

# Only build docker compose, do not start the containers yet
Write-Host '>> Building MCP servers with Docker Compose...'
if ($toolsWithEnv.Count -gt 0) {
    $composeServices = $toolsWithEnv | ForEach-Object { "tool-$_" }
    Write-Host "Building services: $($composeServices -join ', ')"
    $dockerComposeArgs = @('compose', '-f', 'src/docker-compose.yml', 'up', '--build', '-d') + $composeServices
    & docker @dockerComposeArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Warning: Docker Compose build had issues (exit code $LASTEXITCODE). This is not critical for the local development environment setup."
        # Don't exit here as Docker issues shouldn't block the entire setup
    } else {
        Write-Host "Docker Compose build completed successfully."
    }
} else {
    Write-Host "No tools with .env files found, skipping Docker Compose build."
}
