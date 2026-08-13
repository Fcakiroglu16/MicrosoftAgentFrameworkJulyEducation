FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8088
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AdvancedAgentDeploy.csproj", "AdvancedAgentDeploy/"]
RUN dotnet restore "./AdvancedAgentDeploy/AdvancedAgentDeploy.csproj"
COPY . ./AdvancedAgentDeploy
WORKDIR "/src/AdvancedAgentDeploy"
RUN dotnet build "./AdvancedAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AdvancedAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvancedAgentDeploy.dll"]
