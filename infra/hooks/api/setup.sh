#!/bin/bash

# API Setup Script
# This script installs dependencies for the API service and sets up Docker components.
# Line ending handling: This file uses Unix line endings (LF) enforced by .gitattributes
# to prevent Windows CRLF issues that cause '$'\r': command not found' errors.

# Install dependencies for the API service
printf ">> Installing dependencies for the API service...\n"
if [ ! -d ./src/api/node_modules ]; then
    printf "Installing dependencies for the API service...\n"
    npm ci --prefix=src/api --legacy-peer-deps
    status=$?
    if [ $status -ne 0 ]; then
        printf "API dependencies installation failed with exit code $status. Exiting.\n"
        exit $status
    fi
else
    printf "Dependencies for the API service already installed.\n"
fi

# Enable Docker Desktop Model Runner
printf ">> Enabling Docker Desktop Model Runner...\n"
if command -v docker >/dev/null 2>&1; then
    docker desktop enable model-runner --tcp 12434 || printf "Warning: Could not enable Docker Desktop Model Runner. This might be expected if Docker Desktop is not available.\n"
else
    printf "Warning: Docker not found. Skipping Docker Desktop Model Runner setup.\n"
fi

printf ">> Pulling Docker model...\n"
if command -v docker >/dev/null 2>&1; then
    docker model pull ai/phi4:14B-Q4_0 || printf "Warning: Could not pull Docker model. This might be expected if Docker Desktop Model Runner is not available.\n"
else
    printf "Warning: Docker not found. Skipping Docker model pull.\n"
fi

printf ">> API setup completed successfully.\n"
