# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ETAM.Domain/ETAM.Domain.csproj", "src/ETAM.Domain/"]
COPY ["src/ETAM.Application/ETAM.Application.csproj", "src/ETAM.Application/"]
COPY ["src/ETAM.Infrastructure/ETAM.Infrastructure.csproj", "src/ETAM.Infrastructure/"]
COPY ["src/ETAM.Web/ETAM.Web.csproj", "src/ETAM.Web/"]
RUN dotnet restore "src/ETAM.Web/ETAM.Web.csproj"
COPY . .
RUN dotnet publish "src/ETAM.Web/ETAM.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render fournit le port à écouter via la variable PORT (8080 par défaut en local).
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Exécution sans privilèges root (bonne pratique de sécurité).
RUN adduser --disabled-password --gecos "" etam && chown -R etam /app
USER etam

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ETAM.Web.dll"]
