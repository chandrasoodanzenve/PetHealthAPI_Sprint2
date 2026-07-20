#  PetHealth Pro - Intelligence Prioritization Framework

## 1. Signal Prioritization Matrix

We rank incoming data signals based on their impact on business outcomes (Retention & Revenue).

| Priority Level       | Signal Category       | Example Metric                             | Action Urgency        |
| :------------------- | :-------------------- | :----------------------------------------- | :-------------------- |
| **Tier 1: Critical** | Financial & Lifecycle | Subscription Cancel, Payment Failure       | Immediate (Real-time) |
| **Tier 2: Warning**  | Negative Engagement   | 7 days of inactivity, Drop in Health Score | High (24-48 hours)    |
| **Tier 3: Growth**   | Feature Adoption      | Added a new pet, Completed vaccination log | Medium (Weekly)       |
| **Tier 4: General**  | Basic Interaction     | App login, Profile view, Settings change   | Low (Monthly)         |

## 2. Intelligence Confidence Levels

To ensure signal reliability, we assign confidence scores based on data source quality.

- **High Confidence (90-100%)**: Direct transactional data from SQL Server (e.g., actual pet record creation).
- **Medium Confidence (60-80%)**: Aggregated behavioral metrics from OpenTelemetry (e.g., time spent on a page).
- **Low Confidence (<50%)**: Derived or estimated signals (e.g., predicted pet age if not provided).

## 3. Conflict Resolution Rules

When two signals provide conflicting information about a customer, we apply the following rules:

1. **Hierarchy Rule**: Financial signals (Revenue domain) always override behavioral signals (Engagement domain).
   - _Example_: User hasn't logged in (Negative), but they just paid for a year (Positive). Result: **Active User**.
2. **Freshness Rule**: The most recent signal takes precedence over historical averages.
   - _Example_: User was a 'Power User' for 6 months but has zero activity in the last 10 days. Result: **Churn Risk**.
3. **Source Integrity Rule**: Hard DB records override log-based estimates.
