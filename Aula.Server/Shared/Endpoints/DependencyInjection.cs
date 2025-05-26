using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Shared.Endpoints;

internal static class DependencyInjection
{
	internal static IServiceCollection AddEndpoints(this IServiceCollection services, Type assemblyType)
	{
		var endpointDescriptors = assemblyType.Assembly.DefinedTypes
			.Where(static t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsInterface: false, IsAbstract: false })
			.Select(static t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

		var apiEndpointDescriptors = assemblyType.Assembly.DefinedTypes
			.Where(static t => t.IsAssignableTo(typeof(IApiEndpoint)) && t is { IsInterface: false, IsAbstract: false })
			.Select(static t => ServiceDescriptor.Transient(typeof(IApiEndpoint), t));

		services.TryAddEnumerable(endpointDescriptors);
		services.TryAddEnumerable(apiEndpointDescriptors);

		_ = services.AddApiVersioning(options =>
		{
			options.ApiVersionReader = new UrlSegmentApiVersionReader();
			options.UnsupportedApiVersionStatusCode = StatusCodes.Status404NotFound;
		});

		return services;
	}

	internal static IServiceCollection AddEndpoints<TAssemblyType>(this IServiceCollection services) =>
		AddEndpoints(services, typeof(TAssemblyType));

	internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
	{
		var endpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

		foreach (var endpoint in endpoints)
			endpoint.Build(builder);

		var apiVersionSet = builder.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.Build();

		var apiGroup = builder.MapGroup("api/v{apiVersion:apiVersion}").WithApiVersionSet(apiVersionSet);

		var apiEndpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IApiEndpoint>>();

		foreach (var endpoint in apiEndpoints)
			endpoint.Build(apiGroup);

		return builder;
	}
}
