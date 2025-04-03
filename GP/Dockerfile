# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GP.csproj", "."]  # Changed path - file is in same directory
RUN dotnet restore "GP.csproj"
COPY . .
RUN dotnet build "GP.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GP.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GP.dll"]
