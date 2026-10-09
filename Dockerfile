# syntax=docker/dockerfile:1

# ---- build: compiles the API, which also publishes the Blazor WASM frontend into wwwroot
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DistribuidoraGustavo.sln dotnet-tools.json ./
COPY DistribuidoraGustavo.API/DistribuidoraGustavo.API.csproj DistribuidoraGustavo.API/
COPY DistribuidoraGustavo.App/DistribuidoraGustavo.App.csproj DistribuidoraGustavo.App/
COPY DistribuidoraGustavo.Core/DistribuidoraGustavo.Core.csproj DistribuidoraGustavo.Core/
COPY DistribuidoraGustavo.Data/DistribuidoraGustavo.Data.csproj DistribuidoraGustavo.Data/
COPY DistribuidoraGustavo.Interfaces/DistribuidoraGustavo.Interfaces.csproj DistribuidoraGustavo.Interfaces/
RUN dotnet restore DistribuidoraGustavo.API/DistribuidoraGustavo.API.csproj

COPY . .
RUN dotnet publish DistribuidoraGustavo.API/DistribuidoraGustavo.API.csproj -c Release -o /app/publish

# ---- migrations-build: self-contained executable that applies the EF Core migrations
FROM build AS migrations-build
RUN dotnet tool restore \
    && dotnet ef migrations bundle \
        --project DistribuidoraGustavo.Data \
        --startup-project DistribuidoraGustavo.API \
        --self-contained \
        -o /app/efbundle

# ---- migrations: one-shot container, receives the connection string as arguments
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations
WORKDIR /app
COPY --from=migrations-build /app/efbundle ./efbundle
ENTRYPOINT ["./efbundle"]

# ---- app: API + frontend
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS app
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "DistribuidoraGustavo.API.dll"]
