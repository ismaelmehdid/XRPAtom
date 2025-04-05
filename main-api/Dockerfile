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

WORKDIR /app
COPY --from=build /app/out ./

# Set environment variables
# Force HTTP and disable HTTPS
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
# This is the critical setting to disable HTTPS redirect
ENV ASPNETCORE_HTTP_PORTS=80
# Disable HTTPS development certificate
ENV DOTNET_GENERATE_ASPNET_CERTIFICATE=false
# Allow ASP.NET Core to run as a non-root user
ENV DOTNET_RUNNING_IN_CONTAINER=true
# Disable the startup messages
ENV DOTNET_NOLOGO=true
# Avoid generating unnecessary files
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Expose port 80
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "XRPAtom.API.dll"]