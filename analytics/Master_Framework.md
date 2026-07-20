#  Unified Customer Intelligence Master Framework

### 1. Overlapping Metrics (Signal Integration)

- **Login (Engagement) + Subscription (Revenue)**: Identifies "Paying but Inactive" users.
- **Pet Count (Adoption) + Failure Rate (Service)**: Identifies "Frustrated Adopters".

### 2. Signal Prioritization Framework

| Rank   | Domain     | Metric         | Weight |
| :----- | :--------- | :------------- | :----- |
| **P1** | Revenue    | Payment Status | 50%    |
| **P2** | Adoption   | Pet Records    | 30%    |
| **P3** | Engagement | App Logins     | 20%    |

### 3. Conflict Resolution Rules

- **Rule 1**: Transactional Data (SQL) > Telemetry Data (Logs).
- **Rule 2**: Freshness - Last 24 hours of data takes priority over 30-day averages.

### 4. Governance Practices

- **Audit**: Weekly review of "Signal Confidence" levels.
- **Privacy**: All intelligence profiles must be anonymized before being sent to analytics partners.
