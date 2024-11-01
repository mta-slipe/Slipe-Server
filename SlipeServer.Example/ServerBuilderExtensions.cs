﻿using SlipeServer.Example.Logic;
using SlipeServer.LuaControllers;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Example;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddExampleLogic(this ServerBuilder builder)
    {
        builder.AddLogic<ServerExampleLogic>();
        builder.AddLogic<LuaControllersExampleLogic>();
        builder.AddLogic<ResourcesExampleLogic>();
        builder.AddLuaControllers();

        return builder;
    }
}
