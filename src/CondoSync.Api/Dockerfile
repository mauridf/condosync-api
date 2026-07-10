FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CondoSync.slnx ./
COPY src/CondoSync.Core/CondoSync.Core.csproj src/CondoSync.Core/
COPY src/CondoSync.Application/CondoSync.Application.csproj src/CondoSync.Application/
COPY src/CondoSync.Infrastructure/CondoSync.Infrastructure.csproj src/CondoSync.Infrastructure/
COPY src/CondoSync.Api/CondoSync.Api.csproj src/CondoSync.Api/
RUN dotnet restore src/CondoSync.Api/CondoSync.Api.csproj

COPY . .
WORKDIR /src/src/CondoSync.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CondoSync.Api.dll"]
