namespace Aula.Server.Shared.Extensions;

internal static class EnumExtensions
{
	internal static IEnumerable<TEnum> GetDefinedFlags<TEnum>(this TEnum e)
		where TEnum : struct, Enum
	{
		var allFlags = Enum.GetValues<TEnum>();
		return allFlags.Where(flag => e.HasFlag(flag));
	}

	internal static Boolean IsEnumFlagDefined<TEnum>(this TEnum value)
		where TEnum : struct, Enum =>
		IsEnumFlagDefined(Convert.ToInt64(value), typeof(TEnum));

	private static Boolean IsEnumFlagDefined(Int64 value, Type enumType)
	{
		Int64 confirmedFlags = 0;
		foreach (var enumFlag in Enum.GetValues(enumType))
		{
			var flagInt = Convert.ToInt64(enumFlag);
			if ((flagInt & value) == flagInt)
			{
				confirmedFlags |= flagInt;
				if (confirmedFlags == value)
					return true;
			}
		}

		return false;
	}
}
