namespace AuthProvider;

public class CamService
{
    public CamService()
    {

    }


    private static string? service;
    private static bool IsRegistered => service != null;
    public static string Service
    {
        get => service ?? throw new Exception("Service has not been registered - please register before use");
        private set
        {
            if (!IsRegistered) service = value;
        }
    }

    private static readonly HashSet<string> Permissions = [];

    public static void RegisterPermission(string permission) => _ = Permissions.Add(permission);

    public static void RegisterService(string service)
    {
        Service = service;

        foreach (string permission in Permissions)
        {
            Console.WriteLine($"{Service} : {permission}");
        }
    }
}
