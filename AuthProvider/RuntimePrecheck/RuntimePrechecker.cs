using AuthProvider.AuthModelBinder;
using AuthProvider.RuntimePrecheck.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.RuntimePrecheck;

public class RuntimePrechecker
{
    public static void RunPrecheck(WebApplication app)
    {
        var prechecker = new ApplicationPrecheckContext(app, "Application");
        prechecker.RunPrecheck();
        // prechecker.PrettyPrint(app.Logger);
        Console.WriteLine(prechecker);
        if (prechecker.ShouldError())
        {
            throw new RuntimePrecheckException();
        }
    }
}
