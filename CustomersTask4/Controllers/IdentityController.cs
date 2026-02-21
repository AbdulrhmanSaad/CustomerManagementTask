using CustomersTask4.UserHandler.Command;
using CustomersTask4.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles =UserRoles.Admin)]
        public async Task<ActionResult> AddRoleToUser(AssignUserRoleCommand command)
        {
            await mediator.Send(command);
            return NoContent();
        }
    }
}
