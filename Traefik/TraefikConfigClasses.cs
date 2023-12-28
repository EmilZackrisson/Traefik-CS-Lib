namespace Traefik;

using System.Text.Json.Serialization;

public class DynamicConfig
{
    public Http Http = null!;
}

public class Router
{

    [JsonPropertyName("EntryPoints")]
    public List<string> EntryPoints { get; set; } = null!;

    [JsonPropertyName("Rule")]
    public string Rule { get; set; } = null!;

    [JsonPropertyName("Middlewares")]

    public List<string>? Middlewares { get; set; } = null!;

    [JsonPropertyName("Service")]
    public string Service { get; set; } = null!;

    [JsonPropertyName("Tls")]
    public Tls? Tls { get; set; } = null!;
}


public class Http
{
    public Dictionary<string, Router> Routers { get; set; } = null!;
    public Dictionary<string, Service> Services { get; set; } = null!;

}

public class Services
{
    public Dictionary<string, Service> ServiceList { get; set; } = null!;
}

public class Service
{
    public LoadBalancer LoadBalancer { get; set; } = null!;
}

public class LoadBalancer
{
    public bool PassHostHeader { get; set; }
    public List<Server> Servers { get; set; } = null!;
}

public class Server
{
    public string Url { get; set; } = null!;
}
public class Tls
{
    public string CertResolver { get; set; } = null!;
}