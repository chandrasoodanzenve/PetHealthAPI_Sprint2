#  Cohort Migration & Transition Matrix

## 1. Migration Logic

We track how users move between behavioral states every 30 days.

| From Cohort  | To Cohort    | Trigger (Indicator)     | Strategy                |
| :----------- | :----------- | :---------------------- | :---------------------- |
| **Seed**     | **Explorer** | Added 1st Pet           | Automated Welcome Badge |
| **Explorer** | **Champion** | > 5 Health Events       | Loyalty Points Unlocked |
| **Champion** | **Explorer** | No activity for 7 days  | Re-engagement SMS       |
| **Explorer** | **Sleeper**  | No activity for 14 days | Churn Prevention Offer  |

## 2. Predictive Stability Assessment

- **High Stability**: 0-5% variation in monthly event count.
- **Low Stability**: >30% drop in login frequency (Predicts Cohort Decline).
