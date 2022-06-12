using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SlipeServer.Example.Elements;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Web.Dto;

namespace SlipeServer.Web.Controllers;

[ApiController]
[Route("Players")]
public class PlayerController : ControllerBase
{
    private readonly IElementCollection elementCollection;
    private readonly IMapper mapper;

    public PlayerController(
        IElementCollection elementCollection,
        IMapper mapper)
    {
        this.elementCollection = elementCollection;
        this.mapper = mapper;
    }

    [HttpGet("")]
    public ActionResult<IEnumerable<PlayerDto>> Get()
    {
        return Ok(this.elementCollection
            .GetByType<CustomPlayer>()
            .Select(x => this.mapper.Map<PlayerDto>(x)));
    }
}
