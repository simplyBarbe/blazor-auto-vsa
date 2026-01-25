# Debug script to capture docker compose execution details
$logPath = "c:\Users\Andrea\source\repos\blazor-auto-vsa\.cursor\debug.log"

# Clear previous log
if (Test-Path $logPath) {
    Remove-Item $logPath -Force
}

function Write-DebugLog {
    param(
        [string]$location,
        [string]$message,
        [hashtable]$data,
        [string]$hypothesisId
    )
    
    $guid = (New-Guid).ToString().Replace("-", "").Substring(0,8)
    $logEntry = @{
        id = "log_$(Get-Date -Format 'yyyyMMddHHmmss')_$guid"
        timestamp = [DateTimeOffset]::Now.ToUnixTimeMilliseconds()
        location = $location
        message = $message
        data = $data
        sessionId = "debug-session"
        runId = "run1"
        hypothesisId = $hypothesisId
    } | ConvertTo-Json -Compress
    
    Add-Content -Path $logPath -Value $logEntry
}

# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:33" -message "Starting docker compose diagnostic" -data @{workingDir = (Get-Location).Path} -hypothesisId "ALL"
# #endregion

# Hypothesis A: Docker daemon not running
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:37" -message "Testing Hypothesis A: Docker daemon status" -data @{} -hypothesisId "A"
# #endregion
try {
    $dockerInfo = docker info 2>&1
    $dockerInfoExitCode = $LASTEXITCODE
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:41" -message "Docker daemon check result" -data @{
        exitCode = $dockerInfoExitCode
        isRunning = ($dockerInfoExitCode -eq 0)
        output = ($dockerInfo -join "`n").Substring(0, [Math]::Min(500, ($dockerInfo -join "`n").Length))
    } -hypothesisId "A"
    # #endregion
} catch {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:49" -message "Docker daemon check error" -data @{
        errorMessage = $_.Exception.Message
        errorType = $_.Exception.GetType().Name
    } -hypothesisId "A"
    # #endregion
}

# Hypothesis B: Docker Compose version/command issue
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:57" -message "Testing Hypothesis B: Docker Compose version" -data @{} -hypothesisId "B"
# #endregion
try {
    $composeVersion = docker compose version 2>&1
    $composeVersionExitCode = $LASTEXITCODE
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:61" -message "Docker Compose version check" -data @{
        exitCode = $composeVersionExitCode
        versionOutput = ($composeVersion -join "`n")
        commandExists = ($composeVersionExitCode -eq 0)
    } -hypothesisId "B"
    # #endregion
} catch {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:69" -message "Docker Compose version check error" -data @{
        errorMessage = $_.Exception.Message
    } -hypothesisId "B"
    # #endregion
}

# Hypothesis C: Port conflicts (8080, 8081, 5432)
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:76" -message "Testing Hypothesis C: Port availability" -data @{} -hypothesisId "C"
# #endregion
$portsToCheck = @(8080, 8081, 5432)
$portStatus = @{}
foreach ($port in $portsToCheck) {
    try {
        $connection = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue -InformationLevel Quiet -ErrorAction SilentlyContinue
        $portStatus[$port] = $connection
        # #region agent log
        Write-DebugLog -location "debug_docker_compose.ps1:83" -message "Port check result" -data @{
            port = $port
            isInUse = $connection
        } -hypothesisId "C"
        # #endregion
    } catch {
        # #region agent log
        Write-DebugLog -location "debug_docker_compose.ps1:90" -message "Port check error" -data @{
            port = $port
            errorMessage = $_.Exception.Message
        } -hypothesisId "C"
        # #endregion
    }
}

# Hypothesis D: Dockerfile path issues
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:99" -message "Testing Hypothesis D: Dockerfile path validation" -data @{} -hypothesisId "D"
# #endregion
$composeFile = "docker-compose.yml"
if (Test-Path $composeFile) {
    $composeContent = Get-Content $composeFile -Raw
    $dockerfilePath = "blazor-auto-vsa\Server\Dockerfile"
    $dockerfileExists = Test-Path $dockerfilePath
    $dockerfileFullPath = if ($dockerfileExists) { (Resolve-Path $dockerfilePath).Path } else { "NOT_FOUND" }
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:107" -message "Dockerfile path check" -data @{
        dockerfilePath = $dockerfilePath
        exists = $dockerfileExists
        fullPath = $dockerfileFullPath
        composeFileExists = $true
    } -hypothesisId "D"
    # #endregion
} else {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:115" -message "docker-compose.yml not found" -data @{} -hypothesisId "D"
    # #endregion
}

# Hypothesis E: Volume mount path issues (Windows paths)
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:121" -message "Testing Hypothesis E: Volume mount paths" -data @{} -hypothesisId "E"
# #endregion
$overrideFile = "docker-compose.override.yml"
if (Test-Path $overrideFile) {
    $overrideContent = Get-Content $overrideFile -Raw
    $userSecretsPath = "$env:APPDATA\Microsoft\UserSecrets"
    $httpsPath = "$env:APPDATA\ASP.NET\Https"
    $userSecretsExists = Test-Path $userSecretsPath
    $httpsExists = Test-Path $httpsPath
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:129" -message "Volume mount path check" -data @{
        userSecretsPath = $userSecretsPath
        userSecretsExists = $userSecretsExists
        httpsPath = $httpsPath
        httpsExists = $httpsExists
        overrideFileExists = $true
    } -hypothesisId "E"
    # #endregion
} else {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:139" -message "docker-compose.override.yml not found" -data @{} -hypothesisId "E"
    # #endregion
}

# Hypothesis F: Docker compose config validation
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:145" -message "Testing Hypothesis F: Docker compose config validation" -data @{} -hypothesisId "F"
# #endregion
try {
    $configOutput = docker compose config 2>&1
    $configExitCode = $LASTEXITCODE
    $configError = ($configOutput | Where-Object { $_ -match "error|Error|ERROR|invalid|Invalid|INVALID" }) -join "`n"
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:150" -message "Docker compose config validation" -data @{
        exitCode = $configExitCode
        isValid = ($configExitCode -eq 0)
        hasErrors = ($configError.Length -gt 0)
        errorMessages = $configError
        outputLength = ($configOutput -join "`n").Length
    } -hypothesisId "F"
    # #endregion
} catch {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:160" -message "Docker compose config validation error" -data @{
        errorMessage = $_.Exception.Message
        errorType = $_.Exception.GetType().Name
    } -hypothesisId "F"
    # #endregion
}

# Hypothesis G: Actual docker compose up attempt
# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:169" -message "Testing Hypothesis G: Docker compose up execution" -data @{} -hypothesisId "G"
# #endregion
try {
    # Try to start docker compose up in detached mode briefly to capture initial errors
    $upOutput = docker compose up -d --build 2>&1
    $upExitCode = $LASTEXITCODE
    $upError = ($upOutput | Where-Object { $_ -match "error|Error|ERROR|failed|Failed|FAILED|cannot|Cannot|CANNOT" }) -join "`n"
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:175" -message "Docker compose up execution result" -data @{
        exitCode = $upExitCode
        succeeded = ($upExitCode -eq 0)
        hasErrors = ($upError.Length -gt 0)
        errorMessages = $upError
        fullOutput = ($upOutput -join "`n")
    } -hypothesisId "G"
    # #endregion
    
    # If it started, stop it immediately
    if ($upExitCode -eq 0) {
        docker compose down 2>&1 | Out-Null
    }
} catch {
    # #region agent log
    Write-DebugLog -location "debug_docker_compose.ps1:188" -message "Docker compose up execution error" -data @{
        errorMessage = $_.Exception.Message
        errorType = $_.Exception.GetType().Name
    } -hypothesisId "G"
    # #endregion
}

# #region agent log
Write-DebugLog -location "debug_docker_compose.ps1:196" -message "Diagnostic script completed" -data @{} -hypothesisId "ALL"
# #endregion

Write-Host "Diagnostic complete. Check log file at: $logPath"
