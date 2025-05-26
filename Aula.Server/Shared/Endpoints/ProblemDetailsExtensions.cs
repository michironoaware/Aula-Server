using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Shared.Endpoints;

internal static class ProblemDetailsExtensions
{
	internal static HttpValidationProblemDetails ToProblemDetails(
		this IEnumerable<ValidationFailure> validationFailures)
	{
		var propertyProblems = new Dictionary<String, List<String>>();

		foreach (var failure in validationFailures)
		{
			if (propertyProblems.TryGetValue(failure.ErrorCode, out var problems))
				problems.Add(failure.ErrorMessage);
			else
				propertyProblems.Add(failure.ErrorCode, [ failure.ErrorMessage ]);
		}

		return new HttpValidationProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation problem",
			Detail = "One or more validation errors occurred.",
			Errors = propertyProblems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray(),
				StringComparer.OrdinalIgnoreCase),
		};
	}
}
