FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8088
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WorkflowAgentDeploy.csproj", "WorkflowAgentDeploy/"]
RUN dotnet restore "./WorkflowAgentDeploy/WorkflowAgentDeploy.csproj"
COPY . ./WorkflowAgentDeploy
WORKDIR "/src/WorkflowAgentDeploy"
RUN dotnet build "./WorkflowAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WorkflowAgentDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowAgentDeploy.dll"]
