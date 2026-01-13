# Multi-stage build for optimized image size
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["MovieReservation.APi/MovieReservation.APi.csproj", "MovieReservation.APi/"]
COPY ["MovieReservation.Data/MovieReservation.Data.csproj", "MovieReservation.Data/"]
COPY ["MovieReservation.Infrastructure/MovieReservation.Infrastructure.csproj", "MovieReservation.Infrastructure/"]
COPY ["MovieReservation.Service/MovieReservation.Service.csproj", "MovieReservation.Service/"]
COPY ["MovieReservation.Sol.ServiceDefaults/MovieReservation.Sol.ServiceDefaults.csproj", "MovieReservation.Sol.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "MovieReservation.APi/MovieReservation.APi.csproj"

# Copy all source code
COPY . .

# Build application
RUN dotnet build "MovieReservation.APi/MovieReservation.APi.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MovieReservation.APi/MovieReservation.APi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 80 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MovieReservation.APi.dll"]