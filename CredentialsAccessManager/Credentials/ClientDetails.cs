using AuthProvider.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CredentialsAccessManager.Credentials;

public class ClientDetails
{

    private ClientDetails(string? clientIp, string? host, string? userAgent) {
	this.ClientIp = clientIp;
	this.Host = host;
	this.UserAgent = userAgent;
    }

    /// <summary>
    ///	Client's initial ip address when session first created.
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// Represents the client's host. ie. ui.unicycleunicorn.net
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Represents the client's user-agent
    /// </summary>
    public string? UserAgent { get; set; }
	
    /// <summary>
    /// Generates the client details from the HttpContext
    public static ClientDetails FromHttpContext(HttpContext context) {
	_ = HeaderUtils.TryGetHeader(context.Request, "Host", out string? ipAddr);
	_ = HeaderUtils.TryGetHeader(context.Request, "X-Real-IP", out string? remote);
	_ = HeaderUtils.TryGetHeader(context.Request, "User-Agent", out string? userAgent);
	return new ClientDetails(ipAddr, remote, userAgent);
    }

    public override string ToString() {
	return $@"Host: {this.Host}
		Ip: {this.ClientIp}
		User Agent: {this.UserAgent}";
    }
}
