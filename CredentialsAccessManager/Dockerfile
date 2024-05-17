FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CredentialsAccessManager/CredentialsAccessManager.csproj", "CredentialsAccessManager/"]
RUN dotnet restore "./CredentialsAccessManager/CredentialsAccessManager.csproj"
COPY . .
WORKDIR "/src/CredentialsAccessManager"
RUN dotnet build "./CredentialsAccessManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CredentialsAccessManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CredentialsAccessManager.dll"]