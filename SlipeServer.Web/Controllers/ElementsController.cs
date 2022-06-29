using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Web.Dto;

namespace SlipeServer.Web.Controllers;

[ApiController]
[Route("Elements")]
public class ElementsController : ControllerBase
{
    private readonly IElementCollection elementCollection;
    private readonly IMapper mapper;

    public ElementsController(
        IElementCollection elementCollection,
        IMapper mapper)
    {
        this.elementCollection = elementCollection;
        this.mapper = mapper;
    }

    [HttpGet("")]
    public ActionResult<IEnumerable<ElementDto>> Get()
    {
        return Ok(this.elementCollection
            .GetAll()
            .Select(x => this.mapper.Map<ElementDto>(x)));
    }
}
