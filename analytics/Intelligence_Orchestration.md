#  Customer Intelligence Orchestration Flow

## 1. Data Pipeline Architecture

The flow of intelligence signals follows this 4-step orchestration:

**Step 1: Signal Capture (Extraction)**

- Capture SQL events (Pet created/deleted).
- Capture API Telemetry (Latency, status codes).

**Step 2: Signal Enrichment (Processing)**

- Attach User Segment tags (e.g., Free vs. Premium).
- Calculate real-time Health Scores using the weights defined in Day 26.

**Step 3: Intelligence Consolidation (Storage)**

- Store consolidated profiles in a dedicated "Intelligence View" in the database.
- Refresh interval: Every 1 hour for standard users, real-time for churn-risk users.

**Step 4: Decision Support (Action)**

- If `ConsolidatedScore < 40`, trigger an automated "Retention Alert" in the Admin Dashboard.
