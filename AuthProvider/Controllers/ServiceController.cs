using AuthProvider;
using AuthProvider.CamInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace AuthProvider.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class ServiceController(IEnumerable<EndpointDataSource> endpointSources, ICamInterface camInterface) : ControllerBase
{
    private readonly IEnumerable<EndpointDataSource> Endpoints = endpointSources;
    private readonly ICamInterface CamInterface = camInterface;

    private static object? Metadata = null;

    [HttpGet]
    public IActionResult GetEndpoints()
    {
        if (Metadata == null)
        {
            var endpoints = Endpoints
            .SelectMany(es => es.Endpoints)
            .OfType<RouteEndpoint>();

            Metadata = BuildEndpointMetadata(endpoints);
        }



        return Ok(Metadata);
    }

    [NonAction]
    private object? BuildEndpointMetadata(IEnumerable<RouteEndpoint> endpoints)
    {
        var output = endpoints.Select(e =>
        {
            var controller = e.Metadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault();

            return new
            {

                ControllerName = controller!.ControllerName.ToLower(),
                ActionName = controller!.ActionName.ToLower(),
                Action = new
                {
                    Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                    Route = $"/{e.RoutePattern.RawText!.TrimStart('/')}",
                    Auth = GetAuthForEndpoint(e),
                }
            };
        });

        Dictionary<string, Dictionary<string, List<object>>> result = [];

        foreach (var item in output)
        {
            if (result.TryGetValue(item.ControllerName, out var actions))
            {
                if (actions.TryGetValue(item.ActionName, out var list))
                    list.Add(item.Action);
                else
                {
                    actions.Add(item.ActionName, [item.Action]);
                }
            }
            else
            {
                result.Add(item.ControllerName, new Dictionary<string, List<object>>() { { item.ActionName, [item.Action] } });
            }
        }

        return new
        {
            Service = CamInterface.ServiceName.ToLower(),
            Endpoints = result,
        };
    }

    [NonAction]
    private static object? GetAuthForEndpoint(RouteEndpoint e)
    {
        //return e.Metadata.GetMetadata<AuthAttribute>()?.GetType().GenericTypeArguments[0].ToString();

        AuthAttribute? authAttribute = e.Metadata.GetMetadata<AuthAttribute>();
        if (authAttribute is null) return null;

        var types = authAttribute.GetType().GetGenericArguments();
        return types.Length switch
        {
            1 => new
            {
                Primary = types[0].Name,
                authAttribute.Permission,
            },
            2 => new
            {
                Primary = types[0].Name,
                Secondary = types[1].Name,
                authAttribute.Permission,
            },
            _ => null,
        };
    }
}
