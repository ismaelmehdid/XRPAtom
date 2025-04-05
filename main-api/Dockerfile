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
ENV ASPNETCORE_URLS=http://+:443
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port 80
EXPOSE 443

# Start the app
ENTRYPOINT ["dotnet", "XRPAtom.API.dll"]