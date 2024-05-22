using System.Net.Http.Json;

namespace AuthProvider.CamInterface;
public class RemoteCamInterface
{
    private HttpClient CamClient = new();
    private string Remote;
    private string Service;

    public RemoteCamInterface(string service, string url)
    {
        Remote = url;
        Service = service;
    }
}
