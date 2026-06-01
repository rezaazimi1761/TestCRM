FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9041

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (better layer caching)
COPY ["AuthService/AuthService.csproj", "AuthService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["nuget.config", "./"]
RUN dotnet restore "AuthService/AuthService.csproj"

# Copy source and publish
COPY Shared/ Shared/
COPY AuthService/ AuthService/
WORKDIR /src/AuthService
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:9041
ENTRYPOINT ["dotnet", "AuthService.dll"]
