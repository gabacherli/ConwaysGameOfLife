## **📖Problem Description**
Extracted from [Wikipedia](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life):

>The universe of the Game of Life is  [an infinite, two-dimensional orthogonal grid of square](https://en.wikipedia.org/wiki/Square_tiling "Square tiling")  _cells_, each of which is in one of two possible states,  _live_  or  _dead_  (or  _populated_  and  _unpopulated_, respectively). Every cell interacts with its eight  _[neighbours](https://en.wikipedia.org/wiki/Moore_neighborhood "Moore neighborhood")_, which are the cells that are horizontally, vertically, or diagonally adjacent. At each step in time, the following transitions occur:
>
>1.  Any live cell with fewer than two live neighbours dies, as if by underpopulation.
>2.  Any live cell with two or three live neighbours lives on to the next generation.
>3.  Any live cell with more than three live neighbours dies, as if by overpopulation.
>4.  Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
>
>The initial pattern constitutes the  _seed_  of the system. The first generation is created by applying the above rules simultaneously to every cell in the seed, live or dead; births and deaths occur simultaneously, and the discrete moment at which this happens is sometimes called a  _tick_.[[nb 1]](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life#cite_note-7)  Each generation is a  _[pure function](https://en.wikipedia.org/wiki/Pure_function "Pure function")_  of the preceding one. The rules continue to be applied repeatedly to create further generations.

## **📌 Overview**
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

## **🚀 Future Improvements**

✅ **Improve Performance with Parallel Processing**
-   Use **multi-threading** for faster board state calculations.


✅ **Add Caching for Frequent Requests**
-   Implement **Redis** to cache frequently requested board states.

✅ **Introduce a UI or Rendering**
-   Build a **visualization** for the board evolution.

## **📌 Performance Considerations**

- When running the simulation, the **total number of cell evaluations** determines performance. For a board of size **rows × columns** running for a number of **iterations**, the **total number of operations** is:
`Operations = iterations × rows × columns`

### 📊 Runtime Estimation in my baseline performance tests: 
- A **20×20 board (400 cells)** achieved **~62,500 iterations per second**. 
- Performance scales **inversely with board size**. Using this benchmark, we can estimate the **iterations per second** for any board size as:
`Iterations/second =~ 25,000,000 / (rows × columns)`
The **expected runtime** in seconds for a number of **iterations** is:
`Expected runtime =~ (iterations × rows × columns) / 25,000,000`
 
### 📊 Example Simulations 
| **Board Size (rows × columns)** | **Iterations** | **Expected runtime** | 
|------------------------|-------------------|-----------------------|
 | **20 × 20 (400 cells)** | **15,000,000** | **~4 minutes** | 
 | **50 × 50 (2,500 cells)** | **15,000,000** | **~25 minutes** | 
 | **400 × 400 (160,000 cells)** | **15,000,000** | **~26.7 hours** | 
 | **1000 × 1000 (1,000,000 cells)** | **15,000,000** | **~6.9 days** | 

## **🎯 Final Notes**

🎉 **You’re all set!** Follow the steps above to run, test, and interact with the API.  
If you need **troubleshooting help**, check logs with:

```sh
docker logs conwaysgameoflife-api-1
docker logs conwaysgameoflife-sqlserver-1
```

## **🗣️ Shoutouts**
- [The Coding Train 🧑🏻‍🔬](https://www.youtube.com/watch?v=FWSR_7kZuYg)
- [Matthew Brown 🤠](https://www.youtube.com/watch?v=0g1PHZdnQcw) - and his website that helped me test small iterations manually https://academo.org/demos/conways-game-of-life/
- [Wikipedia 🤘🏾](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life)
- [My brain 🧠](http://chat.com/) /s
