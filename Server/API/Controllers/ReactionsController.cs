using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Protobuf.Entities;
using Protobuf.Services;
using Protobuf.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ReactionsController(IReactionService reactionService) : ControllerBase
{
  [HttpPost]
  [Authorize]
  public async Task<ActionResult> Create([FromBody] ReactionCreateDto reactionDto)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    return Ok(await reactionService.Create(reactionDto, int.Parse(userId)));
  }

  [HttpDelete("{id}")]
  [Authorize]
  public async Task<ActionResult> Delete([Required] int id)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (userId == null)
    {
      return Unauthorized();
    }

    await reactionService.Delete(new ReactionDeleteDto()
    {
      EmotionCheckInId = id
    }, int.Parse(userId));
    return Ok();
  }
}