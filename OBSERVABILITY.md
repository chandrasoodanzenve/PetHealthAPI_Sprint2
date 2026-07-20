#  PetHealth Pro - Observability & Diagnostics Manual

## 1. Tracing Strategy

- **End-to-End**: API -> Background Worker -> SQL Server.
- **Tool**: Jaeger (Port 16686).

## 2. Metrics & Percentiles (SLIs)

- **P50/P95/P99**: Tracked via `pet_registration_duration` histogram.
- **Throughput**: Measured via request count metrics.

## 3. Intelligent Alerting

- **Anomaly Detection**: Triggered when latency > 500ms.
- **Suppression**: Alerts are suppressed for 10 minutes after initial trigger to prevent fatigue.

## 4. Root Cause Analysis (RCA) Workflow

1. Identify spike in Grafana.
2. Filter TraceID from logs.
3. Inspect Jaeger span for bottleneck identification.
