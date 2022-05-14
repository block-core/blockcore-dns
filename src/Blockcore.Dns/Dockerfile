FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Blockcore.Dns/Blockcore.Dns.csproj", "Blockcore.Dns/"]
RUN dotnet restore "Blockcore.Dns/Blockcore.Dns.csproj"
COPY . .
WORKDIR "/src/Blockcore.Dns"
RUN dotnet build "Blockcore.Dns.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blockcore.Dns.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blockcore.Dns.dll"]
