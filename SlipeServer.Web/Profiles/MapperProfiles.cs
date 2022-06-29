using AutoMapper;
using SlipeServer.Server.Elements;
using SlipeServer.Web.Dto;
using System.Numerics;

namespace SlipeServer.Web.Profiles;

public class MapperProfiles : Profile
{
    public MapperProfiles()
    {
        CreateMap<Player, PlayerDto>();
        CreateMap<Element, ElementDto>();
        CreateMap<Vector3, Vector3Dto>();
    }
}
