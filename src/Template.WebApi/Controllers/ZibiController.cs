using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Zibi;
using Template.WebApi.Contracts.Zibi;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class ZibiController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/v1/zibi
    [HttpGet]
    public async Task<ActionResult<ListZibis.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListZibis.Query(), ct);
        return Ok(result);
    }

    // GET api/v1/zibi/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetZibi.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetZibi.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/v1/zibi
    [HttpPost]
    public async Task<ActionResult<CreateZibi.Response>> Create(
        [FromBody] CreateZibiRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateZibi.Command>(request);
        var result = await mediator.Send(command, ct);

        // 201 with Location pointing at the new resource is the REST-correct response for create.
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    // PUT api/v1/zibi/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateZibi.Response>> Update(
        Guid id,
        [FromBody] UpdateZibiRequest request,
        CancellationToken ct)
    {
        var command = new UpdateZibi.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE api/v1/zibi/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteZibi.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
