# Chaos Engineering Experiment Results

## Experiment 1: SQL Connection Latency

- **Input**: Injected 3000ms delay.
- **Observation**: Middleware detected an anomaly (Logged as ERR).
- **Result**: SUCCESS. System handled the lag without crashing.

## Experiment 2: Pod Failure Simulation

- **Input**: Forced termination of Outbox Processor.
- **Observation**: Kubernetes Liveness Probe triggered a restart.
- **Result**: SUCCESS. Service was restored in 12 seconds.

## Experiment 3: Network Interruption

- **Input**: Simulated 500 error in downstream dependency.
- **Observation**: Polly Retry policy executed 3 attempts.
- **Result**: SUCCESS. Third attempt succeeded, user saw zero errors.
