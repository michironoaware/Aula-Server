using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Shared.Json;

internal static class DependencyInjection
{
	internal static IServiceCollection AddJsonFromAssembly<TAssembly>(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options =>
			options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
		_ = services
			.AddJsonConvertsFromAssembly(typeof(TAssembly).Assembly)
			.AddJsonContextsFromAssembly(typeof(TAssembly).Assembly)
			.AddUInt64EnumToStringConverterForUInt64EnumTypesFromAssembly(typeof(TAssembly).Assembly);
		return services;
	}

	internal static IServiceCollection AddJsonConvertsFromAssembly(
		this IServiceCollection services,
		Assembly assemblyType)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
			var converters = assemblyType.DefinedTypes
				.Where(t => t.BaseType is not null && !t.IsGenericType && t.IsAssignableTo(typeof(JsonConverter)));
			foreach (var converter in converters)
			{
				var instance = Activator.CreateInstance(converter) as JsonConverter ?? throw new UnreachableException();
				options.SerializerOptions.Converters.Add(instance);
			}
		});

		return services;
	}

	internal static IServiceCollection AddJsonContextsFromAssembly(
		this IServiceCollection services,
		Assembly assemblyType)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
			var jsonContexts = assemblyType.DefinedTypes.Where(t => t.BaseType == typeof(JsonSerializerContext));
			foreach (var jsonContextType in jsonContexts)
			{
				var instance = Activator.CreateInstance(jsonContextType) as
					JsonSerializerContext ?? throw new UnreachableException();
				options.SerializerOptions.TypeInfoResolverChain.Add(instance);
			}
		});

		return services;
	}

	internal static IServiceCollection AddUInt64EnumToStringConverterForUInt64EnumTypesFromAssembly(
		this IServiceCollection services,
		Assembly assemblyType)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
			var uint64Enums = assemblyType.DefinedTypes
				.Where(t => t.IsEnum && t.GetEnumUnderlyingType() == typeof(UInt64));
			foreach (var enumType in uint64Enums)
			{
				var instance =
					Activator.CreateInstance(typeof(UInt64EnumToStringConverter<>).MakeGenericType(enumType)) as
						JsonConverter ?? throw new UnreachableException();
				options.SerializerOptions.Converters.Add(instance);
			}
		});

		return services;
	}
}
