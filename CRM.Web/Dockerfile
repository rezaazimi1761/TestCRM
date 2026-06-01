FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9050

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CRM.Web/CRM.Web.csproj", "CRM.Web/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["nuget.config", "./"]
RUN dotnet restore "CRM.Web/CRM.Web.csproj"

COPY Shared/ Shared/
COPY CRM.Web/ CRM.Web/
WORKDIR /src/CRM.Web
RUN dotnet publish "CRM.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:9050
ENTRYPOINT ["dotnet", "CRM.Web.dll"]
