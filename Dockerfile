# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CodePulse.API/CodePulse.API.csproj CodePulse.API/
RUN dotnet restore CodePulse.API/CodePulse.API.csproj

# Copy everything and publish
COPY . .
WORKDIR /src/CodePulse.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create Images directory for any local fallback
RUN mkdir -p /app/Images

COPY --from=build /app/publish .

# Render uses PORT env var (default 10000)
ENV PORT=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CodePulse.API.dll"]
