FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SimpleAgentDeploy.csproj", "SimpleAgentDeploy/"]
RUN dotnet restore "./SimpleAgentDeploy/SimpleAgentDeploy.csproj"
COPY . ./SimpleAgentDeploy
WORKDIR "/src/SimpleAgentDeploy"
RUN dotnet build "./SimpleAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleAgentDeploy.dll"]
