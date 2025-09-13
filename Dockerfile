# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution file
COPY Insight.Invoicing.sln ./

# Copy project files
COPY src/Insight.Invoicing.Domain/Insight.Invoicing.Domain.csproj ./src/Insight.Invoicing.Domain/
COPY src/Insight.Invoicing.Shared/Insight.Invoicing.Shared.csproj ./src/Insight.Invoicing.Shared/
COPY src/Insight.Invoicing.Application/Insight.Invoicing.Application.csproj ./src/Insight.Invoicing.Application/
COPY src/Insight.Invoicing.Infrastructure/Insight.Invoicing.Infrastructure.csproj ./src/Insight.Invoicing.Infrastructure/
COPY src/Insight.Invoicing.API/Insight.Invoicing.API.csproj ./src/Insight.Invoicing.API/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Build and publish the application
RUN dotnet publish src/Insight.Invoicing.API/Insight.Invoicing.API.csproj -c Release -o out

# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published application
COPY --from=build-env /app/out .

# Create logs directory
RUN mkdir -p logs

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Start the application
ENTRYPOINT ["dotnet", "Insight.Invoicing.API.dll"]

