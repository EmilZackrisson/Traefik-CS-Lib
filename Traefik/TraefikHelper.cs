namespace Traefik;

using System.Dynamic;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class TraefikHelper
{
    private readonly YamlStream _yamlStream = new YamlStream();
    private readonly dynamic _config = null!;
    private readonly string _fileName = null!;
    private readonly NLog.Logger _logger = null!;

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
            loggingConfiguration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole);
            NLog.LogManager.Configuration = loggingConfiguration;

            // create a logger
            _logger = NLog.LogManager.GetCurrentClassLogger();

            this._fileName = fileName;
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
        if (node is YamlScalarNode scalar)
        {
            return scalar.Value;
        }
        else if (node is YamlSequenceNode sequence)
        {
            var list = new List<dynamic>();
            foreach (var item in sequence)
            {
                list.Add(ConvertToExpando(item));
            }
            return list;
        }
        else if (node is YamlMappingNode mapping)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;
            foreach (var entry in mapping)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                dictionary[key] = ConvertToExpando(entry.Value);
            }
            return expando;
        }
        else
        {
            throw new NotSupportedException($"Unsupported YamlNode type: {node.GetType()}");
        }
    }

    /// <summary>
    /// Adds a router to the Traefik configuration.
    /// </summary>
    /// <param name="routerName"></param>
    /// <param name="router"></param>
    /// <returns>True if success, oterwise false</returns>
    public bool AddRouter(string routerName, Router router)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            var success = httpConfig.Routers.TryAdd(routerName, router);
            _config.http = httpConfig;

            if (success)
            {
                _logger.Info("Router added");
            }
            else
            {
                _logger.Info("Router not added");
            }

            return success;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error adding router");
            return false;
        }
    }

    /// <summary>
    /// Adds a service to the Traefik configuraton.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="service"></param>
    /// <returns>True if success, oterwise false</returns>
    public bool AddService(string serviceName, Service service)
    {
        try
        {
            var httpConfig = GetHttpObjectFromConfig();
            var success = httpConfig.Services.TryAdd(serviceName, service);
            _config.http = httpConfig;

            if (success)
            {
                _logger.Info("Service added");
            }
            else
            {
                _logger.Info("Service not added");
            }
            return success;
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
    /// <exception cref="ConfigurationInvalidExeption"></exception>
    /// <returns>True if success, oterwise false</returns>
    public bool SaveToFile(string? fileName)
    {
        var saved = false;
        if (ValidateConfig())
        {
            if (fileName == null)
            {
                File.WriteAllText(this._fileName, ConfigToYamlString());
                saved = true;
                _logger.Info("Config saved to file");
            }
            else
            {
                File.WriteAllText(fileName, ConfigToYamlString());
                saved = true;
                _logger.Info("Config saved to file");
            }
        }
        else
        {
            _logger.Error("Config not saved to file");
            throw new ConfigurationInvalidExeption();
        }

        return saved;
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
                if (prop.ToString().Contains("http"))
                {
                    string json = JsonSerializer.Serialize(prop.Value, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null,
                        PropertyNameCaseInsensitive = true
                    });
                    json = json.Replace("\"true\"", "true").Replace("\"false\"", "false");
                    var options = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var httpConfig = JsonSerializer.Deserialize<Http>(json, options);
                    return httpConfig;
                }
            }

            return null; // Add this line to handle the case when the loop doesn't execute
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error getting http object from config");
            return null;
        }
    }

    /// <summary>
    /// Checks that all routers has a service.
    /// </summary>
    /// <returns>True if valid, otherwise false</returns>
    private bool ValidateConfig()
    {
        var validConfig = true;
        var httpConfig = GetHttpObjectFromConfig();

        foreach (var router in httpConfig.Routers)
        {
            var serviceName = router.Value.Service;

            if (!httpConfig.Services.ContainsKey(serviceName) && !serviceName.Contains('@'))
            {
                validConfig = false;
                break; // The config is not valid, no need to check further.
            }
        }

        if (validConfig)
        {
            _logger.Info("Config is valid");
        }
        else
        {
            _logger.Error("Config is not valid");
        }

        return validConfig;
    }

    /// <summary>
    /// Returns a list of all routers keys in the config.
    /// </summary>
    /// <returns></returns>
    public List<string> GetRouters()
    {
        var httpConfig = GetHttpObjectFromConfig();
        var routers = new List<string>();
        foreach (var router in httpConfig.Routers)
        {
            routers.Add(router.Key);
        }

        return routers;
    }

    /// <summary>
    /// Returns a list of all services keys in the config.
    /// </summary>
    /// <returns></returns>
    public List<string> GetServices()
    {
        var httpConfig = GetHttpObjectFromConfig();
        var services = new List<string>();
        foreach (var service in httpConfig.Services)
        {
            services.Add(service.Key);
        }

        return services;
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
            var success = httpConfig.Routers.Remove(routerName);
            _config.http = httpConfig;

            if (success)
            {
                _logger.Info("Router deleted");
            }
            else
            {
                _logger.Info("Router not deleted");
            }
            return success;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error deleting router");
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
            var success = httpConfig.Services.Remove(serviceName);
            _config.http = httpConfig;

            if (success)
            {
                _logger.Info("Service deleted");
            }
            else
            {
                _logger.Info("Service not deleted");
            }

            return success;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error deleting service");
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
        return httpConfig.Routers[routerName];
    }

    /// <summary>
    /// Returns a service object from the config.
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public Service GetService(string serviceName)
    {
        var httpConfig = GetHttpObjectFromConfig();
        return httpConfig.Services[serviceName];
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
            bool success;
                
            if (removed)
            {
                success = httpConfig.Routers.TryAdd(routerName, router);
            }
            else
            {
                success = false;
                _logger.Info("Router not updated. Router not found.");
            }

            if (success)
            {
                _config.http = httpConfig;
                _logger.Info("Router updated. (" + routerName + ")");
            }

            return success;
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
            bool success;

            if (removed)
            {
                success = httpConfig.Services.TryAdd(serviceName, service);
            }
            else
            {
                success = false;
                _logger.Info("Service not updated. " + serviceName + " not found.");
            }

            if (success)
            {
                _config.http = httpConfig;
                _logger.Info("Service updated. (" + serviceName + ")");
            }

            return success;
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
    public List<string> GetMiddlewares()
    {
        var middlewares = new List<string>();

        foreach (var http in _config.http)
        {
            if (http.Key == "middlewares")
            {
                foreach (var middleware in _config.http.middlewares)
                {
                    middlewares.Add(middleware.Key);
                }
            }
        }
        return middlewares;
    }
}