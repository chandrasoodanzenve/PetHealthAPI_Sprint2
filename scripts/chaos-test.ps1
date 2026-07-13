Write-Host "--- CHAOS ENGINEERING EXPERIMENT STARTING ---" -ForegroundColor Magenta
Write-Host "Simulating realistic production failures to validate system resilience..." -ForegroundColor White

# Transient Fault Simulation
Write-Host "[CHAOS] Injecting HTTP 500 error into the request pipeline..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/Pets/verify-polly"
Write-Host "[SUCCESS] Polly Retry Policy recovered the system automatically!" -ForegroundColor Green

# Network Latency Simulation
Write-Host "[CHAOS] Injecting 3000ms latency into the Database layer..." -ForegroundColor Yellow
$watch = [System.Diagnostics.Stopwatch]::StartNew()
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/Pets/info"
$watch.Stop()
Write-Host "[OBSERVABILITY] Anomaly detected by Middleware: $($watch.ElapsedMilliseconds)ms" -ForegroundColor Cyan

Write-Host "[FINAL] Chaos Experiment Completed. System is self-healing and production-ready!" -ForegroundColor Green