
### **📌 Overview**

This project implements **Conway's Game of Life** as a REST API using **.NET 7.0**, **Dapper**, and **SQL Server**. The API allows you to:

-   **Upload a new board state** and store it in the database.
-   **Retrieve the next generation** of a given board.
-   **Fetch a board’s state after X iterations**.
-   **Detect stable patterns or repeating cycles**.
-   **Ensure state persistence across restarts using Docker & SQL Server**.

## **⚡ Dependencies**

Before running the project, ensure you have the following installed:

-   Docker Desktop (for containerized deployment)
-   [PowerShell (Windows) or Bash (Linux/macOS)](https://learn.microsoft.com/en-us/powershell/)
-   [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (for running migrations)

## **🛠️ Setup Instructions**

### **1️⃣ Mount Docker Secrets Locally**

To securely store database credentials, create Docker secrets in the **project root**:

```powershell
mkdir -p secrets
Set-Content -Path secrets/dev_board_sa_password.txt -Value "YourStrong!Passw0rd" -NoNewline
Set-Content -Path secrets/dev_board_rw_db_connection_string.txt -Value "Server=sqlserver;Database=GameOfLife;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true" -NoNewline
Set-Content -Path secrets/dev_board_ro_connection_string.txt -Value "Server=sqlserver;Database=GameOfLife;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true" -NoNewline` 
```
🔹 **Replace `"YourStrong!Passw0rd"` with your preferred password.**  
🔹 These secrets are **mounted in Docker** instead of being stored in configuration files publicly.

### **2️⃣ Start Docker Desktop**

Ensure **Docker Desktop is running** before proceeding.

-   **Windows/macOS:** Open Docker Desktop from the Start menu.
-   **Linux:** Run:
    
```sh    
sudo systemctl start docker`
```

### **3️⃣ Start the API & Database Containers**

To build and start the API and database, run:

```sh
docker-compose up --build` 
```
🔹 This will:

-   Spin up an **SQL Server 2022 container**.
-   Run **database migrations** using `GameOfLife.DbMigrations`.
-   Launch the **GameOfLife.API container**.

### **4️⃣ Verify Database Connection**

After starting the containers, verify that the database is running by executing:

```sh
docker exec -it conwaysgameoflife-sqlserver-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -Q "SELECT name FROM sys.databases;"
```

Expected Output:

```markdown
name
-----------------
GameOfLife
```
If **GameOfLife** is listed, the database is correctly initialized.

## **📌 API Usage**

### **1️⃣ Check API Status**

Once the containers are running, open **Swagger UI**:

```sh
http://localhost:5000/swagger/index.html` 
```
----------

### **2️⃣ Upload a New Board**

#### **Request**

```http
POST /api/boards/upload
Content-Type: application/json

{
  "rows": 5,
  "columns": 5,
  "state": [
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true]
  ]
}
```

#### **Response**
```json
{
  "id": "7dd4c73c-1547-44b6-8838-3d7638e3f8ab"
}
```
----------

### **3️⃣ Get Next Generation of a Board**

#### **Request**

```http
GET /api/boards/{id}/next` 
```

#### **Response**

```json
{
  "rows": 5,
  "columns": 5,
  "state": [
    [false, true, false, true, false],
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true],
    [false, true, false, true, false]
  ]
}
```
----------

### **4️⃣ Get Board State After X Iterations**

#### **Request**

```http
GET /api/boards/{id}/next/{iterations}` 
```

#### **Response**
```json
{
  "rows": 5,
  "columns": 5,
  "state": [
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true]
  ]
}
```
----------

### **5️⃣ Get Stable or Final Iteration After a Determined Number of Iterations**

#### **Request**

```http
GET /api/boards/{id}/finalIteration/{maxIterations}` 
```

#### **Response**
```json
{
  "rows": 5,
  "columns": 5,
  "state": [
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true],
    [false, true, false, true, false],
    [true, false, true, false, true]
  ],
  "iterations": 100,
  "endReason": "Stable"
}
```
----------

## **🚀 Future Improvements**

✅ **Improve Performance with Parallel Processing**

-   Use **multi-threading** for faster board state calculations.

✅ **Add Caching for Frequent Requests**

-   Implement **Redis** to cache frequently requested board states.

✅ **Introduce a UI or Rendering**

-   Build a **visualization** for the board evolution.

----------

## **🎯 Final Notes**

🎉 **You’re all set!** Follow the steps above to run, test, and interact with the API.  
If you need **troubleshooting help**, check logs with:

```sh
docker logs conwaysgameoflife-api-1
docker logs conwaysgameoflife-sqlserver-1
```
