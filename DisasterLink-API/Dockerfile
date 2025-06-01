FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia só o csproj e restaura as dependências
COPY DisasterLink-API.csproj ./
RUN dotnet restore "DisasterLink-API.csproj"

# Copia todo o restante do código
COPY . .

RUN dotnet publish "DisasterLink-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DisasterLink-API.dll"]
