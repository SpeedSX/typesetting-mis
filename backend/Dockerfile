# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY TypesettingMIS.sln ./
COPY TypesettingMIS.API/TypesettingMIS.API.csproj TypesettingMIS.API/
COPY TypesettingMIS.Core/TypesettingMIS.Core.csproj TypesettingMIS.Core/
COPY TypesettingMIS.Infrastructure/TypesettingMIS.Infrastructure.csproj TypesettingMIS.Infrastructure/
COPY TypesettingMIS.Application/TypesettingMIS.Application.csproj TypesettingMIS.Application/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish TypesettingMIS.API/TypesettingMIS.API.csproj -c Release -o /app/publish

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "TypesettingMIS.API.dll"]
