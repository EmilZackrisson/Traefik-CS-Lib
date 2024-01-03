using Traefik;

namespace TraefikTests;

public class Tests
{
    private string testYamlLocation = "/Users/emizac/RiderProjects/Traefik/TraefikTests/test.yaml";
    
    private readonly Router defaultRouter = new Router()
    {
        EntryPoints = new List<string>()
        {
            "websecure"
        },
        Rule = "Host(`test.example.com`)",
        Service = "testService"
    };
    
    private readonly Service defaultService = new Service()
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

    [Test]
    public void TestAddRouter()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var randomName = Guid.NewGuid().ToString();
        var addedRouter = traefik.AddRouter(randomName, defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));

        var router1 = traefik.GetRouter(randomName);
        Assert.That(router1, Is.Not.Null);

    }
    
    [Test]
    public void TestAddService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        
        var addedService = traefik.AddService("testService", defaultService);
        Assert.That(addedService, Is.EqualTo(true));
    }

    [Test]
    public void TestSaveToFile()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        
        var addedRouter = traefik.AddRouter("test", defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        
        var addedService = traefik.AddService("testService", defaultService);
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
        }
    }
    
    [Test]
    public void TestDeleteRouter()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var addedRouter = traefik.AddRouter("test", defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var deletedRouter = traefik.DeleteRouter("test");
        Assert.That(deletedRouter, Is.EqualTo(true));
    }

    [Test]
    public void TestDeleteService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        
        var addedService = traefik.AddService("testServiceDelete", defaultService);
        Assert.That(addedService, Is.EqualTo(true));
        
        var deletedService = traefik.DeleteService("testServiceDelete");
        Assert.That(deletedService, Is.EqualTo(true));
    }

    [Test]
    public void TestGetEntryPoints()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var entryPoints = traefik.GetEntrypoints();
        foreach (var entryPoint in entryPoints)
        {
            Assert.That(entryPoint, Is.Not.Null);
        }
    }

    [Test]
    public void TestUpdateRouter()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
        var addedRouter = traefik.AddRouter("test", defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var updatedRouter = traefik.UpdateRouter("test", new Router()
        {
            EntryPoints = new List<string>()
            {
                "websecure"
            },
            Rule = "Host(`test.example.com`)",
            Service = "testService",
            Tls = new Tls()
            {
                CertResolver = "test"
            }
        });
        Assert.That(updatedRouter, Is.EqualTo(true));
        
        var addedService = traefik.AddService("testService", defaultService);
        Assert.That(addedService, Is.EqualTo(true));

        var valid = traefik.Validate();
        Assert.That(valid, Is.EqualTo(true));
    }
}