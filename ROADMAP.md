# 🗺️ PetHealth Pro - Engineering Roadmap & Tech Debt

## 1. Technical Debt Review

- **Decoupling**: Need to further decouple the monolithic Background Workers into independent Microservices.
- **Messaging**: Transition from SQL-based Outbox to dedicated Message Brokers (RabbitMQ or Azure Service Bus).

## 2. Modernization Opportunities

- **Service Mesh (Istio)**: Implement for advanced traffic management and mTLS security.
- **Serverless Integration**: Migration of vaccination reminder logic to Azure Functions.
- **AI/ML Diagnostics**: Integration of predictive analytics to forecast pet health trends.
