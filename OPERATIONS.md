# Production Operations Framework

## 1. Database Backup & Recovery (Point 3)

- **Backup:** Daily Full SQL Backup scheduled via SQL Agent at 02:00 AM.
- **Recovery:** In case of data loss, restore the latest `.bak` file using SSMS.

## 2. Disaster Recovery & Business Continuity (Point 4)

- **Failover:** If the main server fails, trigger GitHub Actions to redeploy the Docker container to a secondary node.
- **Persistence:** SQL Data is stored in Docker Volumes to prevent loss during container failure.

## 3. Production Runbook (Point 5)

- **Incident: High Latency:** Check Redis cache connectivity.
- **Incident: 500 Error:** Locate the `CorrelationID` from the user and search in `Logs/` directory.
- **Escalation:** Contact Senior DevOps Engineer if DB is unreachable.

# Operational Excellence Framework

## 1. Service Level Indicators (SLI)

Standard metrics used to measure the API's performance:

- **Availability SLI**: (Total successful requests / Total requests) \* 100
- **Latency SLI**: Time taken to return a response for GET requests.
- **Error Rate SLI**: Percentage of 5xx errors returned to the user.

## 2. Service Level Objectives (SLO)

Our target performance goals:
| Metric | SLO Target |
| :--- | :--- |
| **Uptime** | 99.9% availability per month |
| **Response Time** | 95% of requests must be < 500ms (p95) |
| **Success Rate** | < 1% of total requests should result in failure |
| **Audit Compliance**| 100% of Delete/Post actions must have an Audit Log |

## 3. Error Budget

- Monthly allowed downtime: **43.83 minutes**.
- If we exceed this budget, new feature releases are paused to focus on stability.

## 4. Monitoring & Dashboards

We use a centralized dashboarding system (Grafana & Azure App Insights) to track:

- **Reliability**: Real-time visualization of Up/Down status via Health Checks.
- **Deployment Frequency**: Tracking CI/CD success rate in GitHub Actions.
- **Incident Metrics**: Mean Time to Recover (MTTR) tracking.
- **Service Health**: CPU, Memory, and DB connection pool monitoring.
