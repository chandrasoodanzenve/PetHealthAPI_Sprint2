#  PetHealth Pro - Cross-Domain Signal Mapping Model

## 1. Metric Overlap & Domain Integration

We identify how signals from different domains interact to predict user behavior.

| Signal (Metric)         | Primary Domain | Overlapping Domain | Business Impact                                  |
| :---------------------- | :------------- | :----------------- | :----------------------------------------------- |
| **Login Frequency**     | Engagement     | Retention          | High frequency predicts low churn risk.          |
| **Pet Health Reports**  | Adoption       | Retention          | Using core features increases user stickiness.   |
| **Upgrade to Premium**  | Revenue        | Adoption           | High adoption often leads to revenue conversion. |
| **Support Ticket Open** | Service        | Retention          | Unresolved issues signal high churn risk.        |

## 2. Intelligence Profiles (User Segments)

Based on signal mapping, we categorize users into four intelligence profiles:

### A. High Value (Power Users)

- **Signals**: High Engagement + High Adoption + Active Revenue.
- **Action**: Invite to early-access beta features.

### B. Confusion Risk (At-Risk Adoption)

- **Signals**: High Engagement + Low Adoption.
- **Diagnosis**: User logs in but doesn't know how to add pets/records.
- **Action**: Trigger in-app tutorials or onboarding emails.

### C. Churn Risk (Disengaged)

- **Signals**: Low Engagement + Low Adoption + Active Revenue.
- **Diagnosis**: Paying but not using. Likely to cancel next month.
- **Action**: Offer a personalized usage summary or "We miss you" discount.

## 3. Relationship Logic (Score Linkage)

- **Health Score (Adoption)** directly influences **Retention Risk**.
- **Engagement Velocity** is the leading indicator for **Revenue Conversion**.
