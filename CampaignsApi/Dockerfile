# Stage 1: Build stage
# Use SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
# Copy in order of least to most frequently changed (for better caching)
COPY ["CampaignsApi.sln", "./"]
COPY ["CampaignsApi.Domain/CampaignsApi.Domain.csproj", "CampaignsApi.Domain/"]
COPY ["CampaignsApi.Application/CampaignsApi.Application.csproj", "CampaignsApi.Application/"]
COPY ["CampaignsApi.Infrastructure/CampaignsApi.Infrastructure.csproj", "CampaignsApi.Infrastructure/"]
COPY ["CampaignsApi.API/CampaignsApi.API.csproj", "CampaignsApi.API/"]
COPY ["CampaignsApi.UnitTests/CampaignsApi.UnitTests.csproj", "CampaignsApi.UnitTests/"]
COPY ["CampaignsApi.IntegrationTests/CampaignsApi.IntegrationTests.csproj", "CampaignsApi.IntegrationTests/"]


# Restore dependencies
# This layer is cached unless .csproj files change
RUN dotnet restore "CampaignsApi.sln"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/CampaignsApi.API"
RUN dotnet build "CampaignsApi.API.csproj" -c Release -o /app/build

# Stage 2: Publish stage
# Publish the application (creates production-ready output)
FROM build AS publish
RUN dotnet publish "CampaignsApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime stage
# Use smaller runtime image (no SDK needed)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Expose ports
# Port 80 for HTTP (internal container port)
EXPOSE 80
EXPOSE 443

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "CampaignsApi.API.dll"]