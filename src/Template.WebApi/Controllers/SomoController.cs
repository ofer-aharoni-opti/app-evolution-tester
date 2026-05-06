using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Somo;
using Template.WebApi.Contracts.Somo;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class SomoController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/v1/somo
    [HttpGet]
    public async Task<ActionResult<ListSomos.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListSomos.Query(), ct);
        return Ok(result);
    }

    // GET api/v1/somo/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetSomo.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSomo.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/v1/somo
    [HttpPost]
    public async Task<ActionResult<CreateSomo.Response>> Create(
        [FromBody] CreateSomoRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateSomo.Command>(request);
        var result = await mediator.Send(command, ct);

        // 201 with Location pointing at the new resource is the REST-correct response for create.
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    // PUT api/v1/somo/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateSomo.Response>> Update(
        Guid id,
        [FromBody] UpdateSomoRequest request,
        CancellationToken ct)
    {
        var command = new UpdateSomo.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE api/v1/somo/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteSomo.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
