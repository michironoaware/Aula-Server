using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Shared.Json;

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
internal sealed partial class ProblemDetailsJsonContext : JsonSerializerContext;
