using AuthenticationLibrary.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace AuthenticationLibrary.Attributes
{
    public class JwtAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                string token = actionContext.Request.Headers.Authorization.Parameter;
                if (token == null)
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);

                var principal = TokenManager.GetPrincipal(token);
                if (principal == null || !principal.Identity.IsAuthenticated)
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
