$rawInput = [Console]::In.ReadToEnd()

if ([string]::IsNullOrWhiteSpace($rawInput)) {
    $rawInput = ($input | Out-String)
}

if ([string]::IsNullOrWhiteSpace($rawInput)) {
    exit 0
}

try {
    $convertFromJsonCommand = Get-Command ConvertFrom-Json -ErrorAction Stop
    $supportsDepth = $false

    if ($null -ne $convertFromJsonCommand.Parameters) {
        $supportsDepth = $convertFromJsonCommand.Parameters.ContainsKey('Depth')
    }

    if ($supportsDepth) {
        $payload = $rawInput | ConvertFrom-Json -Depth 20
    }
    else {
        $payload = $rawInput | ConvertFrom-Json
    }
}
catch {
    exit 0
}

function Get-FirstValue {
    param(
        [object[]]$Values
    )

    if ($null -eq $Values) {
        return ""
    }

    foreach ($value in $Values) {
        if ($null -ne $value -and -not [string]::IsNullOrWhiteSpace([string]$value)) {
            return [string]$value
        }
    }

    return ""
}

function Get-NestedValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Object,
        [Parameter(Mandatory = $true)]
        [string[]]$Path
    )

    $current = $Object

    foreach ($segment in $Path) {
        if ($null -eq $current) {
            return $null
        }

        if ($current -is [System.Collections.IDictionary]) {
            if (-not $current.Contains($segment)) {
                return $null
            }

            $current = $current[$segment]
            continue
        }

        $property = $current.PSObject.Properties[$segment]
        if ($null -eq $property) {
            return $null
        }

        $current = $property.Value
    }

    return $current
}

$toolName = Get-FirstValue -Values @(
    $payload.toolName,
    $payload.tool_name,
    (Get-NestedValue -Object $payload -Path @('tool', 'name')),
    (Get-NestedValue -Object $payload -Path @('hookEventData', 'toolName'))
)

$commandText = Get-FirstValue -Values @(
    (Get-NestedValue -Object $payload -Path @('toolInput', 'command')),
    (Get-NestedValue -Object $payload -Path @('input', 'command')),
    (Get-NestedValue -Object $payload -Path @('hookEventData', 'toolInput', 'command')),
    (Get-NestedValue -Object $payload -Path @('hookEventData', 'input', 'command'))
)

if ([string]::IsNullOrWhiteSpace($toolName)) {
    exit 0
}

if ($toolName -notmatch 'run_in_terminal|execute') {
    exit 0
}

$dangerousPatterns = @(
    'git\s+reset\s+--hard',
    'git\s+checkout\s+--',
    'git\s+clean\s+-fd',
    'rm\s+-rf\s+/',
    'Remove-Item\s+.*-Recurse\s+.*-Force'
)

foreach ($pattern in $dangerousPatterns) {
    if ($commandText -match $pattern) {
        $denyResult = @{
            hookSpecificOutput = @{
                hookEventName = 'PreToolUse'
                permissionDecision = 'deny'
                permissionDecisionReason = 'Blocked by implementation workflow policy: destructive command pattern detected.'
            }
        }

        $denyResult | ConvertTo-Json -Compress
        exit 0
    }
}

if ($commandText -match '\bdotnet\s+publish\b') {
    $askResult = @{
        hookSpecificOutput = @{
            hookEventName = 'PreToolUse'
            permissionDecision = 'ask'
            permissionDecisionReason = 'Release publish is typically a QA-stage action. Confirm gates have passed.'
        }
    }

    $askResult | ConvertTo-Json -Compress
    exit 0
}

$allowResult = @{
    hookSpecificOutput = @{
        hookEventName = 'PreToolUse'
        permissionDecision = 'allow'
    }
}

$allowResult | ConvertTo-Json -Compress
