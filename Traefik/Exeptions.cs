namespace Traefik;

public class ConfigurationInvalidException : Exception
{
    public ConfigurationInvalidException()
    {
    }

    public ConfigurationInvalidException(string message)
        : base(message)
    {
    }

    public ConfigurationInvalidException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException()
    {
    }

    public ServiceNotFoundException(string message)
        : base(message)
    {
    }

    public ServiceNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class RouterNotFoundException : Exception
{
    public RouterNotFoundException()
    {
    }

    public RouterNotFoundException(string message)
        : base(message)
    {
    }

    public RouterNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class CouldNotAddException : Exception
{
    public CouldNotAddException()
    {
    }

    public CouldNotAddException(string message)
        : base(message)
    {
    }

    public CouldNotAddException(string message, Exception inner)
        : base(message, inner)
    {
    }
}