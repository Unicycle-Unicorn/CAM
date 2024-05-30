using AuthProvider.RuntimePrecheck.Context;

namespace AuthProvider.RuntimePrecheck.Interfaces;

public interface IActionPrecheckAttribute
{
    public void RunActionPrecheck(ActionPrecheckContext context);
}
