# Day 34: Disaster Recovery (DR) Drill Report

## 1. Drill Scenario

- **Simulated Event**: Complete outage of Azure "East US" Region (Primary).
- **Date**: July 8, 2026.

## 2. Timeline & Outcomes (Validation)

| Time  | Action                                           | Outcome                        |
| :---- | :----------------------------------------------- | :----------------------------- |
| 14:00 | Forced shut down of East US App Service.         | Monitoring alerts triggered.   |
| 14:02 | Traffic Manager health probe failed for Primary. | Health status: Unhealthy.      |
| 14:03 | **Failover Triggered**: DNS updated to West US.  | Traffic redirected to DR Site. |
| 14:05 | SQL Failover Group promoted West US as Primary.  | Write operations restored.     |

## 3. Resilience Metrics

- **Actual RTO (Downtime)**: 3 Minutes (Target: <30 mins).
- **Actual RPO (Data Loss)**: 0 Minutes (Asynchronous replication was sync).

## 4. Conclusion

The automated failover logic implemented in Terraform is validated and functional.

## 5. Failback Validation (Return to Primary)

| Time  | Action                                              | Outcome                          |
| :---- | :-------------------------------------------------- | :------------------------------- |
| 16:00 | East US Services restored & verified.               | Health probes: Healthy.          |
| 16:15 | Initiated Data Re-sync (Failback).                  | Data consistency: 100% Verified. |
| 16:30 | **Failback Completed**: Traffic shifted to East US. | DNS updated successfully.        |

## 6. Dependency Disruption Test

- **Scenario**: SQL Database down, but API is up.
- **Outcome**: **Polly Circuit Breaker** tripped within 5 failures. Traffic Manager health probe failed. System remained stable without crashing the App Service.
