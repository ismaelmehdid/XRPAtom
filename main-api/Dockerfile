# Use the official .NET SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and all project files first
# This allows for better layer caching
COPY *.sln ./
COPY XRPAtom.API/*.csproj ./XRPAtom.API/
COPY XRPAtom.Blockchain/*.csproj ./XRPAtom.Blockchain/
COPY XRPAtom.Core/*.csproj ./XRPAtom.Core/
COPY XRPAtom.Infrastructure/*.csproj ./XRPAtom.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY XRPAtom.API/. ./XRPAtom.API/
COPY XRPAtom.Blockchain/. ./XRPAtom.Blockchain/
COPY XRPAtom.Core/. ./XRPAtom.Core/
COPY XRPAtom.Infrastructure/. ./XRPAtom.Infrastructure/

# Build and publish the app
WORKDIR /app
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime as the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install curl for the healthcheck (as it's not included in the base image)
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/out ./

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 80
EXPOSE 80

# Configure health check for container orchestration
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Start the app
ENTRYPOINT ["dotnet", "XRPAtom.API.dll"]