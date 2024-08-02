using AuthProvider.AuthModelBinder;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace AuthProvider.Exceptions;
public class ExceptionDetails
{
    public Exception Exception;
    public Guid Guid;
    public string Username;
    public string Url;
    public DateTime Time;

    public ExceptionDetails(Exception exception, HttpContext context)
    {
        Exception = exception;
        Guid = Guid.NewGuid();
        Url = context.Request.GetDisplayUrl();
        Time = DateTime.UtcNow;

        if (ItemUtils.TryGet<AuthUsername, string>(context, out string username))
        {
            Username = username;
        }
    }

    public override string ToString()
    {
        var s = new StringBuilder("An exception occured during an http request to: ");
        _ = s.AppendLine(Url);
        _ = s.Append("Guid: ").AppendLine(Guid.ToString());
        _ = s.Append("Time: ").AppendLine(Time.ToString());
        _ = s.Append("Username: ").AppendLine(Username);
        _ = s.Append(Exception);
        return s.ToString();
    }
}
