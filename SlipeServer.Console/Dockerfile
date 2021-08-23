#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SlipeServer.Console/SlipeServer.Console.csproj", "SlipeServer.Console/"]
COPY ["SlipeServer.Lua/SlipeServer.Lua.csproj", "SlipeServer.Lua/"]
COPY ["SlipeServer.Scripting/SlipeServer.Scripting.csproj", "SlipeServer.Scripting/"]
COPY ["SlipeServer.Server/SlipeServer.Server.csproj", "SlipeServer.Server/"]
COPY ["SlipeServer.Net/SlipeServer.Net.csproj", "SlipeServer.Net/"]
COPY ["SlipeServer.Packets/SlipeServer.Packets.csproj", "SlipeServer.Packets/"]
COPY ["SlipeServer.ConfigurationProviders/SlipeServer.ConfigurationProviders.csproj", "SlipeServer.ConfigurationProviders/"]
RUN dotnet restore "SlipeServer.Console/SlipeServer.Console.csproj"
COPY . .
WORKDIR "/src/SlipeServer.Console"
RUN dotnet build "SlipeServer.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SlipeServer.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 50666/udp
EXPOSE 50789/udp
EXPOSE 50667/udp
EXPOSE 40680/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SlipeServer.Console.dll"]