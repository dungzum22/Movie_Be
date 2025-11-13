# syntax=docker/dockerfile:1

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["PostBackend.csproj", "/src/"]
RUN dotnet restore "/src/PostBackend.csproj"

# Copy everything and publish
COPY . /src
RUN dotnet publish "/src/PostBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Connection string is provided via environment variable:
#   ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..."
# On Render, this is set automatically when you link a PostgreSQL database.

ENTRYPOINT ["dotnet", "PostBackend.dll"]
