using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Zaba;
using Template.WebApi.Contracts.Zaba;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public sealed class ZabaController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/v1/zaba
    [HttpGet]
    public async Task<ActionResult<ListZabas.Response>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new ListZabas.Query(), ct);
        return Ok(result);
    }

    // GET api/v1/zaba/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetZaba.Response>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetZaba.Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/v1/zaba
    [HttpPost]
    public async Task<ActionResult<CreateZaba.Response>> Create(
        [FromBody] CreateZabaRequest request,
        CancellationToken ct)
    {
        var command = mapper.Map<CreateZaba.Command>(request);
        var result = await mediator.Send(command, ct);

        // 201 with Location pointing at the new resource is the REST-correct response for create.
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    // PUT api/v1/zaba/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateZaba.Response>> Update(
        Guid id,
        [FromBody] UpdateZabaRequest request,
        CancellationToken ct)
    {
        var command = new UpdateZaba.Command(id, request.Name, request.Description);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE api/v1/zaba/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteZaba.Command(id), ct);
        return result.Deleted ? NoContent() : NotFound();
    }
}
