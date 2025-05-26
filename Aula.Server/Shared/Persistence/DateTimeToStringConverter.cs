using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aula.Server.Shared.Persistence;

internal sealed class DateTimeToStringConverter : ValueConverter<DateTime, String>
{
	public DateTimeToStringConverter()
		: base(ToProvider(), FromProvider())
	{ }

	private static Expression<Func<DateTime, String>> ToProvider()
	{
		return static d => d.ToString("O");
	}

	private static Expression<Func<String, DateTime>> FromProvider()
	{
		return static s => DateTime.ParseExact(s, "O", CultureInfo.InvariantCulture,
			DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
	}
}
