FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MatchmakingService.API/MatchmakingService.API.csproj", "MatchmakingService.API/"]
COPY ["MatchmakingService.Abstractions/MatchmakingService.Abstractions.csproj", "MatchmakingService.Abstractions/"]
COPY ["MatchmakingService.Core/MatchmakingService.Core.csproj", "MatchmakingService.Core/"]
COPY ["MatchmakingService.Application/MatchmakingService.Application.csproj", "MatchmakingService.Application/"]
COPY ["MatchmakingService.DependencyInjection/MatchmakingService.DependencyInjection.csproj", "MatchmakingService.DependencyInjection/"]
COPY ["MatchmakingService.Infrastructure/MatchmakingService.Infrastructure.csproj", "MatchmakingService.Infrastructure/"]
RUN dotnet restore "MatchmakingService.API/MatchmakingService.API.csproj"
COPY . .

WORKDIR "/src/MatchmakingService.API"
RUN dotnet build "MatchmakingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MatchmakingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchmakingService.API.dll"]
