# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SlipeServer.Console/SlipeServer.Console.csproj", "SlipeServer.Console/"]
COPY ["SlipeServer.Example/SlipeServer.Example.csproj", "SlipeServer.Example/"]
COPY ["SlipeServer.ConfigurationProviders/SlipeServer.ConfigurationProviders.csproj", "SlipeServer.ConfigurationProviders/"]
COPY ["SlipeServer.Server/SlipeServer.Server.csproj", "SlipeServer.Server/"]
COPY ["SlipeServer.Net/SlipeServer.Net.csproj", "SlipeServer.Net/"]
COPY ["SlipeServer.Packets/SlipeServer.Packets.csproj", "SlipeServer.Packets/"]
COPY ["SlipeServer.SourceGenerators/SlipeServer.SourceGenerators.csproj", "SlipeServer.SourceGenerators/"]
COPY ["SlipeServer.LuaControllers/SlipeServer.LuaControllers.csproj", "SlipeServer.LuaControllers/"]
COPY ["SlipeServer.Lua/SlipeServer.Lua.csproj", "SlipeServer.Lua/"]
COPY ["SlipeServer.Scripting/SlipeServer.Scripting.csproj", "SlipeServer.Scripting/"]
COPY ["SlipeServer.Luau/SlipeServer.Luau.csproj", "SlipeServer.Luau/"]
COPY ["SlipeServer.Physics/SlipeServer.Physics.csproj", "SlipeServer.Physics/"]
RUN dotnet restore "./SlipeServer.Console/SlipeServer.Console.csproj"
COPY . .
WORKDIR "/src/SlipeServer.Console"
RUN dotnet build "./SlipeServer.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SlipeServer.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
EXPOSE 50666/udp
EXPOSE 50667/udp
EXPOSE 50789/udp
EXPOSE 50790/udp
EXPOSE 40680/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SlipeServer.Console.dll"]
