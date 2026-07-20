#  Behavioral Cohort Evolution & Lifecycle Framework

## 1. Behavioral Cohorts Definition

We group users into cohorts based on their behavioral maturity:

- **Seed Cohort (New)**: Users joined in the last 7 days.
- **Explorer Cohort (Adoption)**: Users with >1 pet but <3 health events.
- **Champion Cohort (Engagement)**: Users with weekly activity and >5 health events.
- **Sleeper Cohort (Risk)**: Users with zero activity in the last 14 days.

## 2. Cohort Migration & Progression Pathways

Tracking how users move between states (Lifecycle Transition):

- **Positive Pathway**: Seed -> Explorer -> Champion (Goal: High-value cohort).
- **Negative Pathway**: Champion -> Explorer -> Sleeper (Signal: Churn risk).

## 3. Behavioral Stability Assessment

We measure "Stability" by checking if a user stays in the Champion cohort for 3 consecutive months.

- **Stability Score = (Days in Target Cohort / Total Days) \* 100**.
