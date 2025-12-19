FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ContosoTravel.ChatAgentService.csproj", "."]
RUN dotnet restore "ContosoTravel.ChatAgentService.csproj"
COPY . .
RUN dotnet build "ContosoTravel.ChatAgentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContosoTravel.ChatAgentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContosoTravel.ChatAgentService.dll"]
