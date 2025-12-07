# CatCollectorAPI

## Project Overview
**CatCollectorAPI** is a RESTful backend API built with **ASP.NET Core 8** and **SQLite** for managing player data in a video game. It handles user authentication, player statistics, and leaderboard management. Players can track:

- Player Name
- Best Score
- Good Cats Collected
- Bad Cats Collected
- Fat Cats Collected (double-score cats)

The API also supports JWT-based authentication for secure operations.

---

## Features

- **User Authentication**
  - Register and login users
  - JWT token-based authentication
  - Role-based access control (Admin/User)

- **Player Management**
  - Create, read, update, and delete players
  - Upsert scores by player name (handles total counts)
  - Fetch all players ordered by best score
  - Fat Cats are counted separately and give double points in-game

- **Cat Types**
  - Get all cat types (`Good`, `Bad`, `Fat`)

- **Database Seeding**
  - Seeds sample players and an admin user on first run
  - Admin credentials: `username: admin`, `password: Admin123!`

---

## Technology Stack

- **Backend:** ASP.NET Core 8
- **Database:** SQLite (via Entity Framework Core)
- **Authentication:** JWT (JSON Web Tokens)
- **Documentation:** Swagger / OpenAPI
- **Tools:** Visual Studio, dotnet CLI

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQLite](https://www.sqlite.org/index.html) (optional for direct DB access)
- [Visual Studio](https://visualstudio.microsoft.com/) or VS Code

### Setup Instructions
1. **Clone the repository**
```bash
https://github.com/IrdinaHassan21/SENG-NurIrdina-Porject3.git
cd CatCollectorAPI
```
2. Install dependencies

dotnet restore


3. Apply database migrations

dotnet ef database update


4. Run the API

dotnet run


5. Access Swagger UI

Navigate to https://localhost:7235/swagger or http://localhost:5074/swagger to explore endpoints.

# Cat Collector API Documentation

## Player Model

Represents a player in the Cat Collector game.

| Property             | Type      | Description                                      |
|----------------------|-----------|--------------------------------------------------|
| `Id`                 | `Guid`    | Unique identifier for the player                |
| `Name`               | `string`  | Player's name (required, max length 100)        |
| `GoodCatsCollected`  | `int`     | Number of good cats collected                   |
| `BadCatsCollected`   | `int`     | Number of bad cats collected                    |
| `FatCatsCollected`   | `int`     | Number of fat (chonky) cats collected          |
| `BestScore`          | `int`     | Player's best score                             |

---

## API Endpoints

### Auth

| Method | Endpoint                 | Description                       | Body |
|--------|-------------------------|-----------------------------------|------|
| POST   | `/api/auth/register`     | Register a new user               | `Username`, `Password`, `Role` (optional) |
| POST   | `/api/auth/login`        | Login and receive JWT             | `Username`, `Password` |

---

### Players

| Method | Endpoint                      | Description                                      | Body / Notes |
|--------|-------------------------------|--------------------------------------------------|---------------|
| GET    | `/api/players`                | Get all players, ordered by BestScore          | None |
| GET    | `/api/players/{id}`           | Get a player by ID                              | None |
| POST   | `/api/players`                | Create a new player (requires JWT)             | `Name` |
| PUT    | `/api/players/{id}`           | Update full player stats (requires JWT)        | `BestScore`, `GoodCatsCollected`, `BadCatsCollected`, `FatCatsCollected` |
| DELETE | `/api/players/{id}`           | Delete a player (Admin only, requires JWT)     | None |
| PUT    | `/api/players/update-score`  | Upsert by name: updates score or creates player (requires JWT) | `Name`, `BestScore`, `GoodCats`, `BadCats`, `FatCats` |

---

### Cat Types

| Method | Endpoint            | Description                     | Notes |
|--------|-------------------|---------------------------------|-------|
| GET    | `/api/cattypes`    | Returns all cat types           | `Good`, `Bad`, `Fat` |

---

## Notes

- Fat Cats give double score — ensure frontend logic reflects this. 
- Passwords are hashed using HMACSHA512. 
- JWT authentication is required for all modifying endpoints. 
- Admin users can delete players; regular users can create/update. 
- CORS is enabled for development (AllowAll policy).  
