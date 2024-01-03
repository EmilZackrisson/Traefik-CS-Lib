using System.ComponentModel.DataAnnotations;

namespace Traefik;

using System.Text.Json.Serialization;

public class DynamicConfig
{
    public Http Http;
}

public class Router
{
    [JsonPropertyName("EntryPoints")]
    public required List<string> EntryPoints { get; set; }

    [JsonPropertyName("Rule")]
    public required string Rule { get; set; }

    [JsonPropertyName("Middlewares")]

    public List<string>? Middlewares { get; set; }
    
    [JsonPropertyName("Service")]
    public required string Service { get; set; }

    [JsonPropertyName("Tls")]
    public Tls? Tls { get; set; }
}


public class Http
{
    public Dictionary<string, Router> Routers { get; set; }
    public Dictionary<string, Service> Services { get; set; }

}

public class Services
{
    public Dictionary<string, Service> ServiceList { get; set; }
}

public class Service
{
    public LoadBalancer LoadBalancer { get; set; }
}

public class LoadBalancer
{
    public bool PassHostHeader { get; set; }
    public required List<Server> Servers { get; set; }
}

public class Server
{
    public required string Url { get; set; }
}
public class Tls
{
    public string CertResolver { get; set; }
}