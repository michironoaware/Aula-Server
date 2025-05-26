using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Shared;

internal static class ValidationHelper
{
	internal static Boolean TryValidate<T>(
		this T instance,
		out ICollection<ValidationResult> results) where T : IValidatableObject
	{
		results = [ ];
		return Validator.TryValidateObject(instance, new ValidationContext(instance), results, true);
	}

	internal static Boolean TryValidate(
		this Object instance,
		out ICollection<ValidationResult> results)
	{
		results = [ ];
		return Validator.TryValidateObject(instance, new ValidationContext(instance), results, true);
	}
}
