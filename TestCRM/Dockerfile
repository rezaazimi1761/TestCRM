FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9040

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TestCRM/TestCRM.csproj", "TestCRM/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["nuget.config", "./"]
RUN dotnet restore "TestCRM/TestCRM.csproj"

COPY Shared/ Shared/
COPY TestCRM/ TestCRM/
WORKDIR /src/TestCRM
RUN dotnet publish "TestCRM.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:9040
ENTRYPOINT ["dotnet", "TestCRM.dll"]
