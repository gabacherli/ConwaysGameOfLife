# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base .NET SDK image for building the projects
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /src

# Copy projects and restore dependencies
COPY src/GameOfLife.API/GameOfLife.API.csproj src/GameOfLife.API/
COPY src/GameOfLife.DbMigrations/GameOfLife.DbMigrations.csproj src/GameOfLife.DbMigrations/
RUN dotnet restore src/GameOfLife.API/GameOfLife.API.csproj
RUN dotnet restore src/GameOfLife.DbMigrations/GameOfLife.DbMigrations.csproj

# Copy everything, build and publish the projects
COPY . ./
RUN dotnet publish src/GameOfLife.API/GameOfLife.API.csproj -c Release -o /out/api
RUN dotnet publish src/GameOfLife.DbMigrations/GameOfLife.DbMigrations.csproj -c Release -o /out/dbmigrations

# API Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS api-runtime
WORKDIR /app
COPY --from=build-env /out/api ./
ENTRYPOINT ["dotnet", "GameOfLife.API.dll"]

# DbMigrations Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS dbmigrations-runtime
WORKDIR /app
COPY --from=build-env /out/dbmigrations ./
COPY src/GameOfLife.DbMigrations/Scripts ./Scripts
ENTRYPOINT ["dotnet", "GameOfLife.DbMigrations.dll"]
