using Eventuras.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Eventuras.WebApi
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var result = GetResult(context.Exception);
            if (result == null)
            {
                return;
            }
            context.Result = result;
            context.ExceptionHandled = true;
        }

        private static IActionResult GetResult(Exception e)
        {
            return e switch
            {
                NotFoundException => new NotFoundObjectResult(e.Message),
                NotAccessibleException => new ForbidResult(),
                DuplicateException => new ConflictObjectResult(e.Message),
                _ => null
            };
        }
    }
}
