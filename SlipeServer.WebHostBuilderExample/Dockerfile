#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SlipeServer.WebHostBuilderExample/SlipeServer.WebHostBuilderExample.csproj", "SlipeServer.WebHostBuilderExample/"]
COPY ["SlipeServer.Console/SlipeServer.Console.csproj", "SlipeServer.Console/"]
COPY ["SlipeServer.ConfigurationProviders/SlipeServer.ConfigurationProviders.csproj", "SlipeServer.ConfigurationProviders/"]
COPY ["SlipeServer.Hosting/SlipeServer.Hosting.csproj", "SlipeServer.Hosting/"]
COPY ["SlipeServer.Server/SlipeServer.Server.csproj", "SlipeServer.Server/"]
COPY ["SlipeServer.Net/SlipeServer.Net.csproj", "SlipeServer.Net/"]
COPY ["SlipeServer.Packets/SlipeServer.Packets.csproj", "SlipeServer.Packets/"]
COPY ["SlipeServer.SourceGenerators/SlipeServer.SourceGenerators.csproj", "SlipeServer.SourceGenerators/"]
COPY ["SlipeServer.LuaControllers/SlipeServer.LuaControllers.csproj", "SlipeServer.LuaControllers/"]
COPY ["SlipeServer.Lua/SlipeServer.Lua.csproj", "SlipeServer.Lua/"]
COPY ["SlipeServer.Scripting/SlipeServer.Scripting.csproj", "SlipeServer.Scripting/"]
COPY ["SlipeServer.Physics/SlipeServer.Physics.csproj", "SlipeServer.Physics/"]
RUN dotnet restore "./SlipeServer.WebHostBuilderExample/./SlipeServer.WebHostBuilderExample.csproj"
COPY . .
WORKDIR "/src/SlipeServer.WebHostBuilderExample"
RUN dotnet build "./SlipeServer.WebHostBuilderExample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SlipeServer.WebHostBuilderExample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 50666/udp
EXPOSE 50789/udp
EXPOSE 50667/udp
EXPOSE 40680/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SlipeServer.WebHostBuilderExample.dll"]