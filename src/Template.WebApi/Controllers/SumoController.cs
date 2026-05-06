using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Sumo;
using Template.WebApi.Contracts.Sumo;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class SumoController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListSumos.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListSumos.Query(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetSumo.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSumo.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateSumo.Response>> Create(
        [FromBody] CreateSumoRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateSumo.Command>(request);
        var result = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateSumo.Response>> Update(
        Guid id,
        [FromBody] UpdateSumoRequest request,
        CancellationToken ct)
    {
        var command = new UpdateSumo.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteSumo.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
