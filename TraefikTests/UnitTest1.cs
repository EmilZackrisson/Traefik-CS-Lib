using Traefik;

namespace TraefikTests;

public class Tests
{
    private string testYamlLocation = "/Users/emizac/RiderProjects/Traefik/TraefikTests/test.yaml";
    /*[SetUp]
    public void Setup()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
    }*/

    [Test]
    public void TestAddRouter()
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
        
    }
    
    [Test]
    public void TestAddService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
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
    }

    [Test]
    public void TestSaveToFile()
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
        
        var deletedRouter = traefik.DeleteRouter("test");
        Assert.That(deletedRouter, Is.EqualTo(true));
    }

    [Test]
    public void TestDeleteService()
    {
        var traefik = new Traefik.TraefikHelper(testYamlLocation);
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
        
        var addedService = traefik.AddService("testServiceDelete", service);
        Assert.That(addedService, Is.EqualTo(true));
        
        var deletedService = traefik.DeleteService("testServiceDelete");
        Assert.That(deletedService, Is.EqualTo(true));
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