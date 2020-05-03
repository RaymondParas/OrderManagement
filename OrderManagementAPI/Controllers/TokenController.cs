using AuthenticationLibrary.Models;
using AuthenticationLibrary.Services;
using System.Web.Http;

namespace OrderManagementAPI.Controllers
{
    [RoutePrefix("api/Token")]
    public class TokenController : ApiController
    {
        [HttpPost]
        public IHttpActionResult GenerateToken([FromBody] TokenIdentification identification)
        {
            if (!ModelState.IsValid ||identification == null || identification.Username == null || identification.Password == null)
                return BadRequest();

            var recognized = TokenManager.IsIdentityRecognized(identification);
            if (!recognized)
                return BadRequest("Not recognized.");

            var token = TokenManager.GenerateToken(identification);

            if (token == null)
                return BadRequest("Invalid token.");

            return Ok(token);
        }
    }
}
