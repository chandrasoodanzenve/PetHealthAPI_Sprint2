#  Action Prioritization & Escalation Model

## 1. Prioritization Matrix

- **Critical (P0)**: Financial churn (Cancellation) or Major health drop.
- **Standard (P1)**: Feature non-adoption.
- **Maintenance (P2)**: General feedback and reviews.

## 2. Escalation & Feedback

- **Escalation**: If `IsEscalationRequired` is TRUE, the recommendation is sent to a Human Account Manager.
- **Feedback Loop**: We track if the user clicked the recommended action. Success updates the `ConfidenceScore`.
