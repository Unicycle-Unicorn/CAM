using PermissionName = string;
using ServiceName = string;

namespace CredentialsAccessManager.Credentials;

public class Permissions
{
    public Dictionary<ServiceName, HashSet<PermissionName>> Perms;

    public Permissions(Dictionary<string, HashSet<string>> permissions)
    {
        Perms = permissions;
    }

    public Permissions()
    {
        Perms = [];
    }

    public Permissions Duplicate()
    {
        var copied = new Permissions();
        foreach ((ServiceName service, HashSet<PermissionName> permissions) in Perms)
        {
            foreach (PermissionName permission in permissions)
            {
                copied.Add(service, permission);
            }
        }
        return copied;
    }

    public bool Contains(ServiceName service, PermissionName permission)
    {
        if (Perms.TryGetValue(service, out var permissions) && permissions != null)
        {
            return permissions.Contains(permission);
        }
        return false;
    }

    public bool Contains((ServiceName service, PermissionName permission) singlePermision)
    {
        return Contains(singlePermision.service, singlePermision.permission);
    }

    public bool Remove((ServiceName service, PermissionName permission) singlePermision)
    {
        return Remove(singlePermision.service, singlePermision.permission);
    }

    public bool Remove(ServiceName service, PermissionName permission)
    {
        if (Perms.TryGetValue(service, out var permissions) && permissions != null)
        {
            return permissions.Remove(permission);
        }

        return false;
    }

    public void Add(ServiceName service, PermissionName permission)
    {
        if (Perms.TryGetValue(service, out var permissions) && permissions != null)
        {
            _ = permissions.Add(permission);
        }
        else
        {
            Perms.Add(service, [permission]);
        }
    }
}
