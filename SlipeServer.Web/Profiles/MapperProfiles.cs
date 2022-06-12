using AutoMapper;
using SlipeServer.Server.Elements;
using SlipeServer.Web.Dto;

namespace SlipeServer.Web.Profiles;

public class MapperProfiles : Profile
{
    public MapperProfiles()
    {
        CreateMap<Player, PlayerDto>();
    }
}
