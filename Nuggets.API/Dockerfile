# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj files and restore
COPY ["Nuggets.API/Nuggets.API.csproj", "Nuggets.API/"]
COPY ["Nuggets.Domain/Nuggets.Domain.csproj", "Nuggets.Domain/"]
COPY ["Nuggets.Application/Nuggets.Application.csproj", "Nuggets.Application/"]
COPY ["Nuggets.Infrastructure/Nuggets.Infrastructure.csproj", "Nuggets.Infrastructure/"]
RUN dotnet restore "Nuggets.API/Nuggets.API.csproj"

# copy everything else and publish
COPY . .
WORKDIR /src/Nuggets.API
RUN dotnet publish "Nuggets.API.csproj" -c Release -o /app/publish

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nuggets.API.dll"]
