# Pet Health Tracker API - Sprint 2 Final

This is a professional .NET 8 Web API developed as part of Sprint 2 to manage pet health records and vitals.

## 🚀 Key Features
- **Full CRUD Support:** Create, Read, Update, and Delete pet records.
- **Multi-Database Support:** Seamlessly switch between **SQLite** and **SQL Server (SSMS)** via configuration.
- **Professional Documentation:** Fully interactive **Swagger UI** for API testing.
- **Stability & Observability:** Implemented **Structured Logging** (ILogger) and **Global Exception Handling Middleware**.
- **Data Integrity:** Applied Data Annotations for server-side validation.

## 🛠️ Tech Stack
- **Framework:** .NET 8.0
- **Database:** MS SQL Server & SQLite
- **ORM:** Entity Framework Core (EF Core)
- **Tools:** Swagger/OpenAPI, VS Code Desktop, Postman

## 📥 Setup & Installation
1. **Clone the repository:**
   `git clone https://github.com/chandrasoodanzenve/PetHealthAPI_Sprint2.git`
2. **Configure Database:**
   Update the connection string in `appsettings.json`.
3. **Apply Migrations:**
   `dotnet ef database update`
4. **Run the Application:**
   `dotnet run`
5. **Access Documentation:**
   Open `http://localhost:5082/swagger` in your browser.

## 📂 Project Structure
- **/Controllers:** REST endpoints logic.
- **/Models:** Data structure and schema definitions.
- **/Data:** Database context and configurations.
- **/Middleware:** Global error handling logic.