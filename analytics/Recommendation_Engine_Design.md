#  Decision Recommendation Engine Design

## 1. Workflow Architecture

1. **Trigger**: Monthly/Weekly cron job or real-time event.
2. **Contextual Evaluation**: Service pulls Health and Cohort scores.
3. **Prescription Generation**: Applies rules defined in `CustomerIntelligenceService`.
4. **Confidence Filtering**: Only actions with >80% ConfidenceScore are sent to users.

## 2. Confidence Scoring Logic

- **Historical Accuracy**: Based on past feedback loops.
- **Data Freshness**: Real-time SQL data gets +20% confidence vs logs.

graph LR
Telemetry[Telemetry Signals] --> Processor[Scenario Evaluator]
Processor --> Rules[Insight-to-Action Rules]
Rules --> Optimizer[Decision Optimizer: Expected Value Calculation]
Optimizer --> Filter[Confidence Filter > 80%]
Filter --> Recommendation[API Output: Prescriptive Action]
Recommendation --> Feedback[User Feedback Loop]
Feedback --> Telemetry
