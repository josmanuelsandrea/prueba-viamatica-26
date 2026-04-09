# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ViamaticaApi.sln .
COPY src/ViamaticaApi.Domain/ViamaticaApi.Domain.csproj src/ViamaticaApi.Domain/
COPY src/ViamaticaApi.Application/ViamaticaApi.Application.csproj src/ViamaticaApi.Application/
COPY src/ViamaticaApi.Infrastructure/ViamaticaApi.Infrastructure.csproj src/ViamaticaApi.Infrastructure/
COPY src/ViamaticaApi.Api/ViamaticaApi.Api.csproj src/ViamaticaApi.Api/

RUN dotnet restore

COPY . .

RUN dotnet publish src/ViamaticaApi.Api/ViamaticaApi.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ViamaticaApi.Api.dll"]
