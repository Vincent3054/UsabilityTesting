FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UsabilityTesting.Worker/UsabilityTesting.Worker.csproj", "UsabilityTesting.Worker/"]
RUN dotnet restore "./UsabilityTesting.Worker/UsabilityTesting.Worker.csproj"
COPY . .
WORKDIR "/src/UsabilityTesting.Worker"
RUN dotnet build "./UsabilityTesting.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UsabilityTesting.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure config directory exists
USER root
RUN mkdir -p /app/config && chown app:app /app/config
USER app
ENTRYPOINT ["dotnet", "UsabilityTesting.Worker.dll"]
