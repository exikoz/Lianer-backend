# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Lianer.Features.API/Lianer.Features.API.csproj", "Lianer.Features.API/"]
RUN dotnet restore "Lianer.Features.API/Lianer.Features.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Lianer.Features.API"
RUN dotnet build "Lianer.Features.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Lianer.Features.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Run (Säker och chiseled miljö för produktion - VG)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS final
WORKDIR /app

# Kopiera publicerade filer och tilldela ägarskap till app-användaren (UID 1654) för rootless execution
COPY --from=publish --chown=app:app /app/publish .

# Tvinga .NET att lyssna på port 8080 (krävs för icke-root-användare)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Kör som icke-root
USER app

ENTRYPOINT ["dotnet", "Lianer.Features.API.dll"]
