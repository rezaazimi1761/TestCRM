FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9050

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Presentation/ModernCRM.Web/ModernCRM.Web.csproj", "CRM.Web/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["nuget.config", "./"]
RUN dotnet restore "src/Presentation/ModernCRM.Web/ModernCRM.Web.csproj"

COPY Shared/ Shared/
COPY src/Presentation/ModernCRM.Web/ src/Presentation/ModernCRM.Web/
WORKDIR /src/src/Presentation/ModernCRM.Web
RUN dotnet publish "ModernCRM.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:9050
ENTRYPOINT ["dotnet", "ModernCRM.Web.dll"]
