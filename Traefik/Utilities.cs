namespace Traefik;

public abstract class Utilities
{
    public static bool BackupConfigFile(string filename)
    {
        var file = File.ReadAllText(filename);
        File.WriteAllText(filename + ".bak", file);

        return File.Exists(filename + ".bak");
    }
}