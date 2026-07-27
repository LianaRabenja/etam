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
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ETAM.Web.dll"]
