using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Zubi;
using Template.WebApi.Contracts.Zubi;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class ZubiController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/v1/zubi
    [HttpGet]
    public async Task<ActionResult<ListZubis.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListZubis.Query(), ct);
        return Ok(result);
    }

    // GET api/v1/zubi/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetZubi.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetZubi.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/v1/zubi
    [HttpPost]
    public async Task<ActionResult<CreateZubi.Response>> Create(
        [FromBody] CreateZubiRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateZubi.Command>(request);
        var result = await mediator.Send(command, ct);

        // 201 with Location pointing at the new resource is the REST-correct response for create.
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    // PUT api/v1/zubi/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateZubi.Response>> Update(
        Guid id,
        [FromBody] UpdateZubiRequest request,
        CancellationToken ct)
    {
        var command = new UpdateZubi.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE api/v1/zubi/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteZubi.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
