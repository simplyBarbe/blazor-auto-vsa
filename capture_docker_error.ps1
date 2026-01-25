# Script to capture exact docker compose error
$logPath = "c:\Users\Andrea\source\repos\blazor-auto-vsa\.cursor\debug.log"

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
        runId = "run2"
        hypothesisId = $hypothesisId
    } | ConvertTo-Json -Compress
    
    Add-Content -Path $logPath -Value $logEntry
}

# #region agent log
Write-DebugLog -location "capture_docker_error.ps1:30" -message "Capturing docker compose error" -data @{command = "docker compose up"} -hypothesisId "ERROR_CAPTURE"
# #endregion

# Stop any existing containers first
# #region agent log
Write-DebugLog -location "capture_docker_error.ps1:35" -message "Stopping existing containers" -data @{} -hypothesisId "ERROR_CAPTURE"
# #endregion
docker compose down 2>&1 | Out-Null

# Try docker compose up and capture ALL output
# #region agent log
Write-DebugLog -location "capture_docker_error.ps1:40" -message "Attempting docker compose up" -data @{} -hypothesisId "ERROR_CAPTURE"
# #endregion

$allOutput = @()
$errorOutput = @()

# Run docker compose up and capture output line by line
$process = Start-Process -FilePath "docker" -ArgumentList "compose", "up", "--build" -NoNewWindow -PassThru -RedirectStandardOutput "docker_stdout.txt" -RedirectStandardError "docker_stderr.txt"

# Wait a bit to capture initial errors
Start-Sleep -Seconds 5

# Check if process is still running
$isRunning = -not $process.HasExited

# #region agent log
Write-DebugLog -location "capture_docker_error.ps1:53" -message "Docker compose process status" -data @{
    isRunning = $isRunning
    processId = $process.Id
    exitCode = if ($process.HasExited) { $process.ExitCode } else { "STILL_RUNNING" }
} -hypothesisId "ERROR_CAPTURE"
# #endregion

# Read error output
if (Test-Path "docker_stderr.txt") {
    $stderr = Get-Content "docker_stderr.txt" -Raw
    # #region agent log
    Write-DebugLog -location "capture_docker_error.ps1:63" -message "Docker compose stderr output" -data @{
        stderr = $stderr
        stderrLength = $stderr.Length
    } -hypothesisId "ERROR_CAPTURE"
    # #endregion
}

# Read stdout output
if (Test-Path "docker_stdout.txt") {
    $stdout = Get-Content "docker_stdout.txt" -Raw
    # #region agent log
    Write-DebugLog -location "capture_docker_error.ps1:72" -message "Docker compose stdout output" -data @{
        stdout = $stdout
        stdoutLength = $stdout.Length
    } -hypothesisId "ERROR_CAPTURE"
    # #endregion
}

# If still running, stop it
if ($isRunning) {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    # #region agent log
    Write-DebugLog -location "capture_docker_error.ps1:81" -message "Stopped docker compose process" -data @{} -hypothesisId "ERROR_CAPTURE"
    # #endregion
}

# Cleanup temp files
Remove-Item "docker_stdout.txt" -ErrorAction SilentlyContinue
Remove-Item "docker_stderr.txt" -ErrorAction SilentlyContinue

Write-Host "Error capture complete. Check log file at: $logPath"
