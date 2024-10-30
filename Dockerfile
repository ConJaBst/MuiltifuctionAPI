# Use the official .NET 8 SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore dependencies
# Replace 'ConnorAPI' with your project folder name
COPY ConnorAPI/*.csproj ./ConnorAPI/
RUN dotnet restore "ConnorAPI/ConnorAPI.csproj"

# Copy the entire project and build it
COPY . ./
RUN dotnet publish "ConnorAPI/ConnorAPI.csproj" -c Release -o /app/out

# Use a lightweight .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build environment
COPY --from=build-env /app/out .

# Expose port 8000
EXPOSE 8000

# Define the entry point for the application
ENTRYPOINT ["dotnet", "ConnorAPI.dll"]
