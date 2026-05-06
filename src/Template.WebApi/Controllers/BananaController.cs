using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Banana;
using Template.WebApi.Contracts.Banana;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class BananaController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListBananas.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListBananas.Query(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetBanana.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBanana.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateBanana.Response>> Create(
        [FromBody] CreateBananaRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateBanana.Command>(request);
        var result = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateBanana.Response>> Update(
        Guid id,
        [FromBody] UpdateBananaRequest request,
        CancellationToken ct)
    {
        var command = new UpdateBanana.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBanana.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
