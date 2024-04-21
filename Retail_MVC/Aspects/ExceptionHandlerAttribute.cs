using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Retail_MVC.Exceptions;

namespace Retail_MVC.Aspects
{
    public class ExceptionHandlerAttribute:ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            Type exceptionType = context.Exception.GetType();
            string message = context.Exception.Message;

            if (exceptionType == typeof(ProductNotFoundException))
            {
                NotFoundObjectResult result = new NotFoundObjectResult(message);
                context.Result = result;
            }
            else if (exceptionType == typeof(ProductAlreadyExsistsException))
            {
                ConflictObjectResult result = new ConflictObjectResult(message);
                context.Result = result;
            }
            else if (exceptionType == typeof(CategoryNotFoundException))
            {
                NotFoundObjectResult result = new NotFoundObjectResult(message);
                context.Result = result;
            }
            else
            {
                StatusCodeResult result = new StatusCodeResult(500);
                context.Result = result;
            }
        }
    }
}
