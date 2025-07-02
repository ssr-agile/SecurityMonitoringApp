## Debugging stage (for VS)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081

## Build stage
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#WORKDIR /src
#COPY ["SecurityMonitoringApp.csproj", "SecurityMonitoringApp/"]
#RUN dotnet restore "SecurityMonitoringApp/SecurityMonitoringApp.csproj"
#COPY . .
#WORKDIR "/src/SecurityMonitoringApp"
#RUN dotnet build "SecurityMonitoringApp.csproj" -c Release -o /app/build

## Publish stage (renamed from 'publish' to avoid confusion)
#FROM build AS publish
#RUN dotnet publish "SecurityMonitoringApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

## Final production stage
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "SecurityMonitoringApp.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SecurityMonitoringApp.csproj", "."]
RUN dotnet restore "SecurityMonitoringApp.csproj"
COPY . .
RUN dotnet publish "SecurityMonitoringApp.csproj" -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SecurityMonitoringApp.dll"]