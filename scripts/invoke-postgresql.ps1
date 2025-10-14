#!/usr/bin/env pwsh

# .SYNOPSIS
# Invoke the PostgreSQL 'pg_ctl' command

# We use this instead of calling pg_ctl directly from the task
param(
    # pg_ctl "mode" to execute, e.g. initdb, start, stop
    [string] $Command,
    # Path to the pgdata folder
    [string] $DataFolder = "data",
    # Additional pg_ctl parameters
    [string[]] $Parameters
)

Import-Module -name ./scripts/postgresql-util.psm1

Invoke-PostgreSQL -Command $Command -DataFolder $DataFolder -Parameters $Parameters
exit $LASTEXITCODE
