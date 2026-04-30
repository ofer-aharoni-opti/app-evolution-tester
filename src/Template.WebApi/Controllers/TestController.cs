using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Features.Test;
using Template.WebApi.Attributes;
using Template.WebApi.Contracts.Test;

namespace Template.WebApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public sealed class TestController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // Application Response shape matches the API — return it directly.
    // No WebApi response contract needed: GetTest.Response IS the API response.
    [HttpGet]
    public async Task<ActionResult<GetTest.Response>> Get([FromQuery] string value, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTest.Query(value), ct);
        return Ok(result);
    }

    // Application Response shape differs from what the API should expose:
    //   CreateTest.Response has (Message, IsProcessed, ProcessedAt as DateTime?)
    //   CreateTestResponse has (Message, ProcessedAt as string)
    // A WebApi response contract is needed to reshape the data for API consumers.
    [HttpPost]
    [RequireUserNameHeader]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<CreateTestResponse>> Post([FromBody] CreateTestRequest request, CancellationToken ct)
    {
        var command = mapper.Map<CreateTest.Command>(request);
        var result = await mediator.Send(command, ct);
        return Ok(mapper.Map<CreateTestResponse>(result));
    }

    // V2 breaking change: ProcessedAt removed from the response.
    [HttpPost]
    [RequireUserNameHeader]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<CreateTestResponseV2>> PostV2([FromBody] CreateTestRequest request, CancellationToken ct)
    {
        var command = mapper.Map<CreateTest.Command>(request);
        var result = await mediator.Send(command, ct);
        return Ok(mapper.Map<CreateTestResponseV2>(result));
    }

    [HttpGet("error")]
    public async Task<ActionResult<GetTest.Response>> GetError([FromQuery] string value, CancellationToken ct)
    {
        if (DateTime.Now.Second % 56 == 0)
        {
            return BadRequest("Simulated error: even second");
        }
        var result = await mediator.Send(new GetTest.Query(value), ct);
        return Ok(result);
    }

    [HttpGet("zibi")]
    public async Task<ActionResult<GetTest.Response>> GetZibi([FromQuery] string value, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTest.Query(value), ct);
        return Ok(result);
    }
}
