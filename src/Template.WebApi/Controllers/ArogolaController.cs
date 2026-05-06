using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Arogola;
using Template.WebApi.Contracts.Arogola;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class ArogolaController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListArogolas.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListArogolas.Query(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetArogola.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetArogola.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateArogola.Response>> Create(
        [FromBody] CreateArogolaRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateArogola.Command>(request);
        var result = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateArogola.Response>> Update(
        Guid id,
        [FromBody] UpdateArogolaRequest request,
        CancellationToken ct)
    {
        var command = new UpdateArogola.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteArogola.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
