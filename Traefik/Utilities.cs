namespace Traefik;

public abstract class Utilities
{
    public static bool BackupConfigFile(string filename)
    {
        var file = File.ReadAllText(filename);
        var newFileName = filename + ".bak-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        File.WriteAllText(newFileName, file);

        return File.Exists(newFileName);
    }
}