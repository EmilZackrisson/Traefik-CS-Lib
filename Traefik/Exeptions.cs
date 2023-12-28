namespace Traefik;

public class ConfigurationInvalidExeption : Exception
{
    public ConfigurationInvalidExeption()
    {
    }

    public ConfigurationInvalidExeption(string message)
        : base(message)
    {
    }

    public ConfigurationInvalidExeption(string message, Exception inner)
        : base(message, inner)
    {
    }
}