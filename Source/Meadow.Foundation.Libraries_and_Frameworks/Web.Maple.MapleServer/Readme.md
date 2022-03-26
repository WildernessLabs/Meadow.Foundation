# Meadow Foundation Maple Web Server

The Maple Web Server is primarily intended to provide RESTful endpoints from a device.  It is modelled after ASP.NET Core and provides an easy to extend architecture with integrated JSON support via `System.Text.Json`.

## Creating Web API Endpoints

A web API consists of one or more request handler classes that derive from RequestHandlerBase:

```csharp
public class MyRequestHandler : RequestHandlerBase
```

## Attribute Routing

Maple determines API call routing based on Attribute routing of handler methods.

Routing is supported to either absolute or relative paths.

### Absolute Routing

If your route begins with a forward slash (`/`) then it is considered an absolute route, and requests will be routed to the provided route regardless of the Handler class name.  

For example, the following will respond to `GET` requests to `http://[meadow.address]/hello`

```csharp
public class MyRequestHandler : RequestHandlerBase
{
    [HttpGet("/hello")]
    public OkObjectResult Hello()
    { ... }
}
```

### Relative Routing

If your route *does not* begin with a forward slash (`/`) then it is considered a relative route, and requests will be routed to the provided route prefixed with an appreviated `RequestHandler` prefix.  The route prefix is determined by using the class name and trimming off any "Requesthandler" suffix.


For example, the following will respond to `GET` requests to `http://[meadow.address]/my/hello`

```csharp
public class MyRequestHandler : RequestHandlerBase
{
    [HttpGet("hello")]
    public OkObjectResult Hello()
    { ... }
}
```

But the following will respond to `GET` requests to `http://[meadow.address]/webapi/hello`

```csharp
public class WebAPI : RequestHandlerBase
{
    [HttpGet("hello")]
    public OkObjectResult Hello()
    { ... }
}
```

### Route Parameters

> NOTE: Maple supports only a *single* parameter in a Route.

Maple supports providing a handler method parameter through the route path.  Parameters are delineated by curly braces, and the parameter name in the route must exactly match the parameter name in the handler method signature.

As an example, a `GET` to the path `http://[meadow.address]/orders/history/1234` would end up calling the following `GetOrderHistory` handler method with a parameter value of `1234`:

```csharp
public class OrdersRequestHandler : RequestHandlerBase
{
	[HttpGet("history/{orderID}")]
	public void GetOrderHistory(int orderID)
	{
	    Debug.WriteLine($"{paramName}");
	}
}
```

Supported parameter types are:

- Numerics (byte, short, int, long, float, double)
- bool
- string
- DateTime
- Guid



## Handler Caching

By default Maple will create a new instance of an API handler for every request received.  If you want your application to reuse the same handler instance, which provides faster handler execution and decreases GC allocation, simply override the `IsResuable` base property and return `true`.

```csharp
public override bool IsReusable => true;

```

## Returning an `IActionResult`

It is recommended that all Handler methods return an `IActionResult` implementation.  Extension methods are provided by Maple for common return objects including, but not limited to, `ActionResult`, `JsonResult`, `OkResult` and `NotFoundResult`.

For example, the following will automatically serialize and return a JSON string array with the proper `content-type` and return code.

```csharp
[HttpGet("/JsonSample")]
public IActionResult GetJsonList()
{
    var names = new List<string> {
        "George",
        "John",
        "Thomas",
        "Benjamin"
    };

    return new JsonResult(names);
}

```
