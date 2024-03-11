using stock_api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden || authorizeResult.Challenged)
        {
            var response = new ObjectResult(new CommonResponse<dynamic>
            {
                Result = false,
                Message = "沒有權限"
            })
            {
                StatusCode = 403
            };

            context.Response.StatusCode = 403;
            return response.ExecuteResultAsync(new ActionContext
            {
                HttpContext = context
            });
        }

        return next(context);
    }
}
