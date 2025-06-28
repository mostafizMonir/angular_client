# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["GoogleLoginApi.csproj", "./"]
RUN dotnet restore "GoogleLoginApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "GoogleLoginApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GoogleLoginApi.csproj" -c Release -o /app/publish

# Build the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "GoogleLoginApi.dll"] 