using Traefik;

namespace TraefikTests;

public class Tests
{
    private readonly string _testYamlLocation = "/Users/emizac/RiderProjects/Traefik/TraefikTests/test.yaml";
    
    private readonly Router _defaultRouter = new Router()
    {
        EntryPoints = ["websecure"],
        Rule = "Host(`test.example.com`)",
        Service = "testService"
    };
    
    private readonly Service _defaultService = new Service()
    {
        LoadBalancer = new LoadBalancer()
        {
            PassHostHeader = true,
            Servers =
            [
                new Server()
                {
                    Url = "https://test.example.com"
                }
            ]
        }
    };
    
    [Test]
    public void TestCreateTraefikHelper()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        Assert.That(traefik, Is.Not.Null);
    }
    
    [Test]
    public void TestBackupConfigFile()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        var newFileName = _testYamlLocation + ".bak-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        var fileExists = File.Exists(newFileName);
        Assert.That(fileExists, Is.EqualTo(true));
        
        // Cleanup
        File.Delete(newFileName);
        
        var fileExistsAfterDelete = File.Exists(newFileName);
        Assert.That(fileExistsAfterDelete, Is.EqualTo(false));
    }

    [Test]
    public void TestAddRouter()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        var randomName = Guid.NewGuid().ToString();
        var addedRouter = traefik.AddRouter(randomName, _defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));

        var router1 = traefik.GetRouter(randomName);
        Assert.That(router1, Is.Not.Null);

    }
    
    [Test]
    public void TestAddService()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        
        var addedService = traefik.AddService("testService", _defaultService);
        Assert.That(addedService, Is.EqualTo(true));
    }

    [Test]
    public void TestSaveToFile()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        
        var addedRouter = traefik.AddRouter("test", _defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        
        var addedService = traefik.AddService("testService", _defaultService);
        Assert.That(addedService, Is.EqualTo(true));
        
        var saved = traefik.SaveToFile("/Users/emizac/RiderProjects/Traefik/TraefikTests/testOut.yaml");
        Assert.That(saved, Is.EqualTo(true));
    }

    [Test]
    public void TestGetRouters()
    {
        var traefik = new TraefikHelper(_testYamlLocation);

        var routers = traefik.GetRouters();

        foreach (var router in routers)
        {
            Assert.That(router, Is.Not.Null);
        }
    }
    
    [Test]
    public void TestDeleteRouter()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        var addedRouter = traefik.AddRouter("test", _defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var deletedRouter = traefik.DeleteRouter("test");
        Assert.That(deletedRouter, Is.EqualTo(true));
    }

    [Test]
    public void TestDeleteService()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        
        var addedService = traefik.AddService("testServiceDelete", _defaultService);
        Assert.That(addedService, Is.EqualTo(true));
        
        var deletedService = traefik.DeleteService("testServiceDelete");
        Assert.That(deletedService, Is.EqualTo(true));
    }

    [Test]
    public void TestGetEntryPoints()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        var entryPoints = traefik.GetEntrypoints();
        foreach (var entryPoint in entryPoints)
        {
            Assert.That(entryPoint, Is.Not.Null);
        }
    }

    [Test]
    public void TestUpdateRouter()
    {
        var traefik = new TraefikHelper(_testYamlLocation);
        var addedRouter = traefik.AddRouter("test", _defaultRouter);
        Assert.That(addedRouter, Is.EqualTo(true));
        
        var updatedRouter = traefik.UpdateRouter("test", new Router()
        {
            EntryPoints = ["websecure"],
            Rule = "Host(`test.example.com`)",
            Service = "testService",
            Tls = new Tls()
            {
                CertResolver = "test"
            }
        });
        Assert.That(updatedRouter, Is.EqualTo(true));
        
        var addedService = traefik.AddService("testService", _defaultService);
        Assert.That(addedService, Is.EqualTo(true));

        var valid = traefik.Validate();
        Assert.That(valid, Is.EqualTo(true));
    }
}