# 🏛️ PetHealth Pro - Architecture Decision Log (ADL)

## Decision 1: Clean Architecture & Repository Pattern

- **Context**: Need for high maintainability and testability.
- **Decision**: Separated business logic from data access.
- **Outcome**: 100% unit test coverage for Service Layer.

## Decision 2: Multi-Region Disaster Recovery

- **Context**: Requirement for 99.9% availability.
- **Decision**: Implemented Azure Traffic Manager and SQL Geo-Replication.
- **Outcome**: RTO reduced to < 30 minutes.

## Decision 3: Event Sourcing & Saga Pattern

- **Context**: Ensuring data consistency across distributed workflows.
- **Decision**: Used an Outbox table and Compensating Transactions.
- **Outcome**: Guaranteed "Exactly-Once" processing and reliable recovery.
