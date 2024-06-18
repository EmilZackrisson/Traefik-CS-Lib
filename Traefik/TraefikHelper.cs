using System.Dynamic;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Traefik;

public class TraefikHelper
{
    private readonly dynamic _config = null!;
    private readonly string _fileName = null!;
    private readonly NLog.Logger _logger = null!;
    private readonly YamlStream _yamlStream = new YamlStream();

    /// <summary>
    /// Creates and initiates the YamlHelper object.
    /// </summary>
    /// <param name="fileName"></param>
    public TraefikHelper(string fileName)
    {
        try
        {
            // create a configuration instance
            var loggingConfiguration = new NLog.Config.LoggingConfiguration();
            // create a console logging target
            var logConsole = new NLog.Targets.ConsoleTarget();
            loggingConfiguration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logConsole);
            NLog.LogManager.Configuration = loggingConfiguration;

            // create a logger
            _logger = NLog.LogManager.GetCurrentClassLogger();

            this._fileName = fileName;

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Could not find config file");
            }

            if (!Utilities.BackupConfigFile(fileName))
            {
                throw new FileNotFoundException("Could not save backup of config file");
            }

            _yamlStream.Load(new StreamReader(this._fileName));

            // Convert the YAML document to an ExpandoObject
            var root = (YamlMappingNode)_yamlStream.Documents[0].RootNode;
            _config = ConvertToExpando(root);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error creating TraefikHelper");
        }
    }

    private static dynamic ConvertToExpando(YamlNode node)
    {
        switch (node)
        {
            case YamlScalarNode scalar:
                return scalar.Value;
            case YamlSequenceNode sequence:
                return sequence.Select(item => ConvertToExpando(item)).ToList();
            case YamlMappingNode mapping:
            {
                var expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando!;
                foreach (var entry in mapping)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;
                    dictionary[key!] = ConvertToExpando(entry.Value);
                }

                return expando;
            }
            default:
                throw new NotSupportedException($"Unsupported YamlNode type: {node.GetType()}");
        }
    }


    /// <summary>
    /// Adds a router to the Traefik configuration.
    /// </summary>
    /// <param name="routerName"></param>
    /// <param name="router"></param>
    /// <returns>True if success, otherwise false</returns>
    public bool AddRouter(string routerName, Router router)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            if (!httpConfig.Routers.TryAdd(routerName, router))
            {
                throw new CouldNotAddException("Could not add router" + routerName);
            }

            _logger.Debug(router.ToString());

            _config.http.routers = httpConfig.Routers;


            _logger.Info("Router added");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error adding router");
            return false;
        }
    }

    /// <summary>
    /// Adds a service to the Traefik configuration.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="service"></param>
    /// <returns>True if success, otherwise false</returns>
    public bool AddService(string serviceName, Service service)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            if (!httpConfig.Services.TryAdd(serviceName, service))
            {
                throw new CouldNotAddException("Could not add service");
            }

            _logger.Debug(service.ToString());

            _config.http.services = httpConfig.Services;

            _logger.Info("Service added");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error adding service");
            return false;
        }
    }

    /// <summary>
    /// Saves the current settings to file. If no file name is specified, then it updates the same file it read from.
    /// </summary>
    /// <param name="fileName"></param>
    /// <exception cref="ConfigurationInvalidException"></exception>
    /// <returns>True if success, otherwise false</returns>
    public bool SaveToFile(string? fileName)
    {
        try
        {
            if (!this.Validate())
            {
                throw new ConfigurationInvalidException("Config is not valid. Cannot save to file.");
            }

            File.WriteAllText(fileName ?? this._fileName, ConfigToYamlString());

            _logger.Info("Config saved to file");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error saving config to file");
            return false;
        }
    }

    /// <summary>
    /// Converts config to a YAML string used for writing to file.
    /// </summary>
    /// <returns>YAML-string</returns>
    private string ConfigToYamlString()
    {
        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithIndentedSequences()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithQuotingNecessaryStrings()
            .Build();
        string yaml = serializer.Serialize(_config);
        return yaml;
    }

    private Http GetHttpObjectFromConfig()
    {
        try
        {
            var tempConfig = new ExpandoObject() as IDictionary<string, object>;

            foreach (var kvp in _config as IDictionary<string, object>)
            {
                tempConfig.Add(kvp);
            }

            foreach (var prop in tempConfig)
            {
                if (!prop.ToString().Contains("http")) continue;
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                var json = JsonSerializer.Serialize(prop.Value, jsonSerializerOptions);
                json = json.Replace("\"true\"", "true").Replace("\"false\"", "false");
                var httpConfig = JsonSerializer.Deserialize<Http>(json, jsonSerializerOptions);

                if (httpConfig == null)
                {
                    throw new ConfigurationInvalidException();
                }

                return httpConfig;
            }

            throw new ConfigurationInvalidException();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error getting http object from config");
            throw;
        }
    }

    /// <summary>
    /// Validates the config.
    /// </summary>
    /// <returns>True if config is valid, otherwise false.</returns>
    /// <exception cref="ConfigurationInvalidException"></exception>
    public bool Validate()
    {
        try
        {
            var routerNames = GetRouters();
            var serviceNames = GetServices();
            var middlewares = GetMiddlewares();
            var entrypoints = GetEntrypoints();

            foreach (var router in routerNames.Select(GetRouter))
            {
                if (router.EntryPoints == null || router.Rule == null || router.Service == null)
                {
                    throw new ConfigurationInvalidException("Router is missing required fields");
                }

                if (router.EntryPoints.Any(entryPoint => !entrypoints.Contains(entryPoint)))
                {
                    throw new ConfigurationInvalidException("Router is using a non-existing entrypoint");
                }

                if (router.Middlewares != null &&
                    router.Middlewares.Any(middleware => !middlewares.Contains(middleware)))
                {
                    throw new ConfigurationInvalidException("Router is using a non-existing middleware");
                }

                if (!serviceNames.Contains(router.Service) &&
                    !router.Service.Contains('@')) // If the service contains '@' it is a Traefik service.
                {
                    throw new ConfigurationInvalidException("Router is using a non-existing service");
                }
            }

            _logger.Info("Config is valid");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error validating config");
            return false;
        }
    }

    /// <summary>
    /// Returns a IEnumerable of all routers keys in the config.
    /// </summary>
    /// <returns></returns>
    public List<string> GetRouters()
    {
        var httpConfig = GetHttpObjectFromConfig();

        IEnumerable<string> routers = from router in httpConfig.Routers select router.Key;

        var list = routers.ToList();
        _logger.Debug($"Got routers ({string.Join(",", list)})");

        return list;
    }

    /// <summary>
    /// Returns a IEnumerable of all services keys in the config.
    /// </summary>
    /// <returns></returns>
    public List<string> GetServices()
    {
        var httpConfig = GetHttpObjectFromConfig();

        var services = from service in httpConfig.Services select service.Key;

        var enumerable = services.ToList();
        _logger.Debug($"Got services ({string.Join(",", enumerable)})");

        return enumerable;
    }

    /// <summary>
    /// Deletes a router from the config.
    /// </summary>
    /// <param name="routerName"></param>
    /// <returns></returns>
    public bool DeleteRouter(string routerName)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();

            if (!httpConfig.Routers.Remove(routerName))
            {
                throw new RouterNotFoundException();
            }

            _config.http.routers = httpConfig.Routers;

            _logger.Info(routerName + " router deleted");

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error deleting router: " + routerName);
            return false;
        }
    }

    /// <summary>
    /// Deletes a service from the config.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public bool DeleteService(string serviceName)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();

            if (!httpConfig.Services.Remove(serviceName))
            {
                throw new ServiceNotFoundException();
            }

            _config.http.services = httpConfig.Services;

            _logger.Info(serviceName + " service deleted");

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error deleting service: " + serviceName);
            return false;
        }
    }

    /// <summary>
    /// Returns a router object from the config.
    /// </summary>
    /// <param name="routerName"></param>
    /// <returns></returns>
    public Router GetRouter(string routerName)
    {
        var httpConfig = GetHttpObjectFromConfig();
        httpConfig.Routers.TryGetValue(routerName, out var router);

        return router ?? throw new RouterNotFoundException();
    }

    /// <summary>
    /// Returns a service object from the config.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public Service GetService(string serviceName)
    {
        var httpConfig = GetHttpObjectFromConfig();
        httpConfig.Services.TryGetValue(serviceName, out var service);

        return service ?? throw new ServiceNotFoundException();
    }

    /// <summary>
    /// Updates a router in the config.
    /// </summary>
    /// <param name="routerName"></param>
    /// <param name="router"></param>
    /// <returns>True if updated, false if the router don't exists or an error occurs</returns>
    public bool UpdateRouter(string routerName, Router router)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            var removed = httpConfig.Routers.Remove(routerName);

            if (!removed)
            {
                throw new RouterNotFoundException();
            }

            var success = httpConfig.Routers.TryAdd(routerName, router);

            if (!success)
            {
                throw new CouldNotAddException("Could not add router");
            }

            _config.http.routers = httpConfig.Routers;

            _logger.Info("Router updated. (" + routerName + ")");

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating router" + routerName);
            return false;
        }
    }

    /// <summary>
    /// Updates a service in the config.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="service"></param>
    /// <returns>True if updated, false if the service don't exists or an error occurs</returns>
    public bool UpdateService(string serviceName, Service service)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            var removed = httpConfig.Services.Remove(serviceName);

            if (!removed)
            {
                throw new ServiceNotFoundException();
            }

            var success = httpConfig.Services.TryAdd(serviceName, service);

            if (!success)
            {
                throw new CouldNotAddException("Could not add service");
            }

            _config.http.services = httpConfig.Services;

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating service (" + serviceName + ")");
            return false;
        }
    }

    /// <summary>
    /// Returns a list of all middlewares keys in the config.
    /// </summary>
    /// <returns>A list of middlewares in the config</returns>
    public HashSet<string> GetMiddlewares()
    {
        var middlewaresSet = new HashSet<string>();
        var routers = GetRouters();

        foreach (var routerName in routers)
        {
            var router = GetRouter(routerName);

            if (router.Middlewares == null) continue;
            foreach (var middleware in router.Middlewares)
            {
                middlewaresSet.Add(middleware);
            }
        }

        return middlewaresSet;
    }

    /// <summary>
    /// Returns a list of all used entrypoints keys in the config.
    /// </summary>
    /// <returns>A list of all used entrypoints keys in the config.</returns>
    public HashSet<string> GetEntrypoints()
    {
        var entrypoints = new HashSet<string>();
        var routers = GetRouters();

        foreach (var routerName in routers)
        {
            var router = GetRouter(routerName);

            foreach (var entryPoint in router.EntryPoints)
            {
                entrypoints.Add(entryPoint);
            }
        }

        return entrypoints;
    }
}