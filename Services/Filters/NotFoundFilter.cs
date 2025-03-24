using App.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace App.Services.Filters
{
    public class NotFoundFilter<T, TId>(IGenericRepository<T, TId> genericRepository) : Attribute, IAsyncActionFilter 
        where T : class
        where TId : struct

    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idValue) || idValue is not TId id)
            {
                await next();
                return;
            }

            if (!await genericRepository.AnyAsync(id))
            {
                string entityName = typeof(T).Name;
                string actionName = context.ActionDescriptor.RouteValues["action"] ?? "Bilinmeyen Aksiyon";

                var errorMessage = $"Aradığınız {entityName} ({id}) bulunamadı. ({actionName})";

                context.Result = new NotFoundObjectResult(ServiceResult.Fail(errorMessage));
                return;
            }

            await next();
        }
    }
}
