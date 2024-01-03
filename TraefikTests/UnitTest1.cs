using Traefik;
using Traefik.Contracts;

namespace TraefikTests;

public class Tests
{
    private string testYamlLocation = "/Users/emizac/RiderProjects/Traefik/TraefikTests/test.yaml";
    /*[SetUp]
    public void Setup()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
    }*/
    
    private readonly Traefik.Contracts.HttpConfiguration.Router defaultTestRouter = new Traefik.Contracts.HttpConfiguration.Router()
    {
        EntryPoints = ["websecure"],
        Rule = "Host(`test.example.com`)",
        Service = "testService"
    };
    
    private readonly Traefik.Contracts.HttpConfiguration.LoadBalancerHttpService defaultTestService = new Traefik.Contracts.HttpConfiguration.LoadBalancerHttpService()
    {
        LoadBalancer = new Traefik.Contracts.HttpConfiguration.LoadBalancer()
        {
            Servers = new[]
            {
                new Traefik.Contracts.HttpConfiguration.Server()
                {
                    Url = "10.10.0.99"
                }
            }
        }
    };

    [Test]
    public void TestAddRouter()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var addedRouter = traefik.AddRouter("test", defaultTestRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
    }
    
    [Test]
    public void TestAddService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var addedService = traefik.AddService("testService", defaultTestService);
        Assert.That(addedService, Is.EqualTo(true));
    }

    [Test]
    public void TestSaveToFile()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        //
        // var router = new Router()
        // {
        //     EntryPoints = new List<string>()
        //     {
        //         "websecure"
        //     },
        //     Rule = "Host(`test.example.com`)",
        //     Service = "testService"
        // };

        var addedRouter = traefik.AddRouter("test", defaultTestRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var addedService = traefik.AddService("testService", defaultTestService);
        Assert.That(addedService, Is.EqualTo(true));
        
        var saved = traefik.SaveToFile("/Users/emizac/RiderProjects/Traefik/TraefikTests/testOut.yaml");
        Assert.That(saved, Is.EqualTo(true));
    }

    [Test]
    public void TestGetRouters()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);

        var routers = traefik.GetRouters();

        foreach (var router in routers)
        {
            Assert.That(router, Is.Not.Null);
            Console.WriteLine(router);
        }
    }
    
    [Test]
    public void TestDeleteRouter()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        
        var addedRouter = traefik.AddRouter("test", defaultTestRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var deletedRouter = traefik.DeleteRouter("test");
        Assert.That(deletedRouter, Is.EqualTo(true));
    }

    [Test]
    public void TestDeleteService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        
        var addedService = traefik.AddService("testServiceDelete", defaultTestService);
        Assert.That(addedService, Is.EqualTo(true));
        
        var deletedService = traefik.DeleteService("testServiceDelete");
        Assert.That(deletedService, Is.EqualTo(true));
    }

    [Test]
    public void TestUpdateRouter()
    {
        var traefik = new TraefikHelper(testYamlLocation);
        // var router = new Router()
        // {
        //     EntryPoints = new List<string>()
        //     {
        //         "websecure"
        //     },
        //     Rule = "Host(`auth.emilzackrisson.se`)",
        //     Service = "authentik-kubernetes"
        // };
        traefik.UpdateRouter("authentik", defaultTestRouter);
        
        var routers = traefik.GetRouters();
        foreach (var router1 in routers)
        {
            Console.WriteLine(router1);
            Assert.That(router1, Is.Not.Null);
        }
        
        var middlewares = traefik.GetMiddlewares();
        foreach (var middleware in middlewares)
        {
            Console.WriteLine(middleware);
            Assert.That(middleware, Is.Not.Null);
        }
    }

    [Test]
    public void TestGetEntryPoints()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var entryPoints = traefik.GetEntrypoints();
        foreach (var entryPoint in entryPoints)
        {
            Assert.That(entryPoint, Is.Not.Null);
            Console.WriteLine(entryPoint);
        }
    }

    [Test]
    public void TestGetMiddleware()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var middlewares = traefik.GetMiddlewares();
        foreach (var middleware in middlewares)
        {
            Assert.That(middleware, Is.Not.Null);
            Console.WriteLine(middleware);
        }
        
    }
    
    /*
    [Test]
    public void TestValidation()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var router = new Router()
        {
            EntryPoints = new List<string>()
            {
                "websecure"
            },
            Rule = "Host(`test.example.com`)",
            Service = "testService"
        };
        var addedRouter = traefik.AddRouter("test", router);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var service = new Service()
        {
            LoadBalancer = new LoadBalancer()
            {
                PassHostHeader = true,
                Servers = new List<Server>()
                {
                    new Server()
                    {
                        Url = "http://test.example.com"
                    }
                }
            }
        };
        
        var addedService = traefik.AddService("testService", service);
        Assert.That(addedService, Is.EqualTo(true));
        
        var valid = traefik.Validate();
        Assert.That(valid, Is.EqualTo(true));
    }*/
}