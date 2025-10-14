# .SYNOPSIS
# Given one or more candidate command names,
# return the first name that resolves.
function Find-Command {
    param (
        [string[]] $CommandNames
    )

    foreach ($command in $CommandNames) {
        if (get-command $command -ErrorAction SilentlyContinue) {
            return $command
        }
    }
    Write-Error "Couldn't find any of the following : $CommandNames"
    exit 100
}

# .SYNOPSIS
# Find the name of the pg_ctl, which depends on the installation method
function Find-pg_ctl {
    param ()
    return Find-Command(
        "pg_ctl",     # Winget/EDB install postgresql utilities as is (unversioned)
        "pg_ctl-17"   # Homebrew installs versioned aliases of the postgresql utilities
    )
}
Export-ModuleMember -Function Find-pg_ctl

# .SYNOPSIS
# Invoke the PostgreSQL 'pg_ctl' command
function Invoke-PostgreSQL {
    param(
        # pg_ctl "mode" to execute, e.g. initdb, start, stop
        [string] $Command,
        # Path to the pgdata folder
        [string] $DataFolder = "data",
        # Additional pg_ctl parameters
        [string[]] $Parameters
    )

    $pg_ctl = Find-pg_ctl

    &$pg_ctl $Command "--pgdata=$DataFolder" @Parameters
}
Export-ModuleMember -Function Invoke-PostgreSQL

# .SYNOPSIS
# Generate a random password
function GeneratePassword {
    param(
        # Desired length of the generated password
        [ValidateRange(12, 256)]
        [int]
        $length = 14
    )
    $characters = 'a'..'z' + 'A'..'Z' + '0'..'9' + '!@#$%^&*'.ToCharArray()
    $password = $characters | Get-Random -Count $length | Join-String
    return $password
}
Export-ModuleMember -Function GeneratePassword
