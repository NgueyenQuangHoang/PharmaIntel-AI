// =============================================================================
// Filter: ValidationFilter
// Chuc nang: Tu dong resolve IValidator<T> cho moi argument cua action va validate.
// Neu validation fail -> throw ValidationException -> ExceptionHandler tra 400.
// =============================================================================
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using PharmaIntel.Core.Exceptions;
using AppValidationException = PharmaIntel.Core.Exceptions.ValidationException;

namespace PharmaIntel.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sp = context.HttpContext.RequestServices;

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (sp.GetService(validatorType) is not IValidator validator) continue;

            var ctxValidate = new ValidationContext<object>(arg);
            var result = await validator.ValidateAsync(ctxValidate);
            if (result.IsValid) continue;

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => string.IsNullOrEmpty(g.Key) ? "_" : char.ToLowerInvariant(g.Key[0]) + g.Key[1..],
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new AppValidationException(errors);
        }

        await next();
    }
}
