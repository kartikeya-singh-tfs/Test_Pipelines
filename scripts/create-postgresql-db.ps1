#!/usr/bin/env pwsh
# .SYNOPSIS
# Create a PostgreSQL database

param(
    # Folder where the database is stored, will be created if necessary
    [string] $DataFolder = "data",

    # TCP/IP port where PostgreSQL listens
    [int] $Port = 5433,

    # Absolute path to directory for log files (defaults to $DataFolder/logs)
    [string] $LogFolder = $null,

    # Force the creation of the database even if it already exists
    [switch] $Force = $false
)

Import-Module -name ./scripts/postgresql-util.psm1

# Create / recreate the data folder
# The data folder must be empty for `initdb` to succeed.
if (Test-Path $DataFolder/*) {
    if (!$Force) {
        Write-Error "Data folder '$DataFolder' already exists"
        exit 1
    }
    Write-Warning "Removing existing data folder $DataFolder"
    Remove-Item -Recurse -Force $DataFolder
}
$DataFolderFullPath = (New-Item -Path $DataFolder -ItemType Directory -Force).FullName

# Generate a password
# TODO: store this password so that .NET can use it (#17)
$PasswordFile = ".pg_password"
GeneratePassword | Out-File -Encoding ascii $PasswordFile

# Create the database
Invoke-PostgreSQL -Command "initdb" -DataFolder $DataFolder (
    "--options",
    "--username=opal --pwfile=$PasswordFile --auth=scram-sha-256 --encoding=UTF8")

# Create the log folder
if (!$LogFolder) {
    $LogFolder = Join-Path -Path $DataFolderFullPath -ChildPath "logs"
}
New-Item -Path $LogFolder -ItemType Directory -Force | Out-Null

# Update configuration
$ConfigurationFile = "$DataFolder/postgresql.conf"
(Get-Content $ConfigurationFile) `
    -replace "#port = 5432", "port = $Port" `
    -replace "#logging_collector = off", "logging_collector = on" `
    -replace "#log_directory = 'log'", "log_directory = '$LogFolder'" |
Out-File -Encoding ascii $ConfigurationFile

# Add a gitignore file to prevent people from accidentally committing the data folder
# (if it is located in the repository tree)
"*" | Out-File $DataFolder/.gitignore
