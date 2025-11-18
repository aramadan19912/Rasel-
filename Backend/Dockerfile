# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["OutlookInboxManagement.csproj", "./"]
RUN dotnet restore "OutlookInboxManagement.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "OutlookInboxManagement.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "OutlookInboxManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install necessary dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create directory for uploads and logs
RUN mkdir -p /app/uploads /app/logs && chmod 777 /app/uploads /app/logs

# Expose port
EXPOSE 9090

# Set environment variables
ENV ASPNETCORE_URLS=http://+:9090
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:9090/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "OutlookInboxManagement.dll"]
