# Install dependencies for the API service
Write-Host '>> Installing dependencies for the API service...'
$nodeModules = './src/api/node_modules'
if (-not (Test-Path $nodeModules)) {
    Write-Host 'Installing dependencies for the API service...'
    npm ci --prefix=src/api --legacy-peer-deps
    if ($LASTEXITCODE -ne 0) {
        Write-Host "API dependencies installation failed with exit code $LASTEXITCODE. Exiting."
        exit $LASTEXITCODE
    }
} else {
    Write-Host 'Dependencies for the API service already installed.'
}

# Enable Docker Desktop Model Runner (only if Docker Desktop is available)
Write-Host '>> Enabling Docker Desktop Model Runner...'
try {
    $dockerVersionOutput = docker version --format json 2>$null | ConvertFrom-Json
    if ($dockerVersionOutput.Server.Platform.Name -like "*Docker Desktop*") {
        docker desktop enable model-runner --tcp 12434 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host 'Docker Desktop Model Runner enabled successfully.'
        } else {
            Write-Host 'Warning: Failed to enable Docker Desktop Model Runner. This may require Docker Desktop.'
        }
        
        Write-Host '>> Pulling Docker model...'
        docker model pull ai/phi4:14B-Q4_0 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host 'Docker model pulled successfully.'
        } else {
            Write-Host 'Warning: Failed to pull Docker model. This may require Docker Desktop with model support.'
        }
    } else {
        Write-Host 'Skipping Docker Desktop specific commands (not running Docker Desktop).'
    }
} catch {
    Write-Host 'Warning: Could not determine Docker version. Skipping Docker Desktop specific commands.'
}
