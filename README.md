# 🐾 Pet Health Tracker API - Professional Edition (Sprint 3 Complete)

This is a production-ready .NET 8 Web API designed for managing pet health records with high security, clean architecture, and automated workflows.

## 🚀 Key Features (Updated)

- **🔐 JWT Authentication:** Secure login & registration system with Role-Based Access Control (Admin only deletion).
- **🏗️ Clean Architecture:** Implemented **Repository Pattern** and **Service Layer** for better maintainability.
- **✅ Advanced Validation:** Strict data integrity using **FluentValidation** (e.g., specific rules for pet breeds).
- **📊 Standardized Responses:** Uniform API response structure (`IsSuccess`, `Message`, `Data`, `Errors`).
- **🧪 Automated Testing:** Comprehensive **Unit Tests (Moq)** and **Integration Tests** using xUnit.
- **🐳 Containerization:** Fully Dockerized setup for consistent deployment across environments.
- **🤖 CI/CD Pipeline:** Automated Build and Test execution via **GitHub Actions**.

## 🛠️ Tech Stack

- **Framework:** .NET 8.0 (C#)
- **Database:** MS SQL Server (Primary) & SQLite (Portable)
- **Security:** JWT Bearer Token
- **Validation:** FluentValidation
- **Testing:** xUnit, Moq, WebApplicationFactory
- **DevOps:** Docker, Docker Compose, GitHub Actions

## 📥 Setup & Installation

### Option 1: Standard Run

1. **Clone the repository:**
   `git clone https://github.com/chandrasoodanzenve/PetHealthAPI.git`
2. **Apply Migrations:**
   `dotnet ef database update`
3. **Run the Application:**
   `dotnet run`

### Option 2: Docker Run (Recommended)

1. Ensure Docker Desktop is running.
2. Run: `docker-compose up --build`
3. API is accessible at: `http://localhost:5000/swagger`

## 📂 Project Structure

- **/Controllers:** API Request/Response handlers.
- **/Services:** Business logic layer.
- **/Repositories:** Data access abstraction.
- **/Validators:** FluentValidation rules.
- **/Models:** Data schemas and API Response wrappers.
- **/PetHealthAPI.Tests:** Unit and Integration test suite.

## 🤖 CI/CD Details

Every code push to `main` triggers a GitHub Action that:

1. Restores Dependencies.
2. Builds the Solution.
3. Runs all Automated Tests.

## 📊 Observability & Monitoring

The API is now equipped with an end-to-end monitoring system using **OpenTelemetry**.

### 🔍 Key Features:

- **Distributed Tracing:** Integrated with **Jaeger** to visualize the request lifecycle.
- **Custom Metrics:** Tracks **Cache Hit/Miss ratios** and **Background Job execution times**.
- **Critical Alerting:** Automatic logging of high latency (>500ms) and system failures.
- **Dashboard:** Access Jaeger UI at `http://localhost:16686`.

---

## 🐳 Advanced Containerization & Environments

The application is fully containerized and supports seamless switching between multiple environments.

### 🚀 Optimized Docker Setup:

- **Multi-stage Dockerfile:** Reduced image size by separating the Build (SDK) and Runtime (ASP.NET) stages.
- **Startup Validation:** The API service waits for **SQL Server** and **Redis** to become **Healthy** before launching.

### ⚙️ Multi-Environment Deployment:

To run the API in a specific environment, use the following commands:

1. **Development (Local):** `dotnet run`
2. **Staging (Testing):** `dotnet run --environment Staging`
3. **Production (Live):** `dotnet run --environment Production`

### 🛠️ Local Container Setup:

To spin up the entire enterprise stack (API + SQL + Redis + Jaeger):

```bash
docker-compose up --build
```
