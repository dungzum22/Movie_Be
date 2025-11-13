# syntax=docker/dockerfile:1

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# PORT will be set by Render at runtime
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["PostBackend/PostBackend.csproj", "PostBackend/"]
RUN dotnet restore "PostBackend/PostBackend.csproj"

# Copy everything and publish
COPY PostBackend/ PostBackend/
WORKDIR "/src/PostBackend"
RUN dotnet publish "PostBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Connection string is provided via environment variable:
#   ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..."
# On Render, this is set automatically when you link a PostgreSQL database.

ENTRYPOINT ["dotnet", "PostBackend.dll"]

