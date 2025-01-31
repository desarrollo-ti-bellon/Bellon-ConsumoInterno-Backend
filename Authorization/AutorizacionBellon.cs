using System.Text.Json.Serialization;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bellon.API.ConsumoInterno.Authorization;

public class AutorizacionBellon : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not object)
        {
            if (context.HttpContext.User.Identity != null)
            {
                var user = context.HttpContext.User.Identity.Name;
                var bellonAuthService =
                    context.HttpContext.RequestServices.GetRequiredService<IServicioAutorizacion>();

                if (!await bellonAuthService.ValidarUsuarioPerfil(user!))
                {
                    context.Result = new ForbidResult();
                }
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
