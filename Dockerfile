# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/ProjectTemplate.Infrastructure/ProjectTemplate.Infrastructure.csproj", "src/ProjectTemplate.Infrastructure/"]
COPY ["src/ProjectTemplate.Web/ProjectTemplate.Web.csproj", "src/ProjectTemplate.Web/"]

RUN dotnet restore "src/ProjectTemplate.Web/ProjectTemplate.Web.csproj"

COPY . .

RUN dotnet publish "src/ProjectTemplate.Web/ProjectTemplate.Web.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/Logs /app/data && chown -R $APP_UID:$APP_UID /app

USER $APP_UID

ENTRYPOINT ["dotnet", "ProjectTemplate.Web.dll"]
