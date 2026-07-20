#  Entity Resolution & Identity Mapping Principles

## 1. Primary Identifiers

To resolve identities across domains, we use a multi-key mapping strategy:

- **SQL Identity**: `UserID` (Primary Key in Users Table).
- **Behavioral Identity**: `CorrelationID` (Propagated via Middleware).
- **Telemetry Identity**: `TraceID` (from OpenTelemetry).

## 2. Resolution Logic (The Glue)

We link disparate signals using the following mapping rules:

1. **Join Key**: Every API log must contain the `CorrelationID`.
2. **Lookup Table**: A background task maps `CorrelationID` to the authenticated `UserID` at the time of the request.
3. **Session Stitching**: All logs within a 30-minute window of a single `UserID` are stitched into a single "Customer Intelligence Session."

## 3. Conflict Resolution (conflicting signals)

If a signal shows a user as "Active" in logs but "Inactive" in Subscription:

- **Rule**: Transactional integrity (SQL) > Behavioral telemetry (Logs).
