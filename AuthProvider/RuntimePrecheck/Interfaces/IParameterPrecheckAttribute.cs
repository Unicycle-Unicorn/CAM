using AuthProvider.RuntimePrecheck.Context;

namespace AuthProvider.RuntimePrecheck.Interfaces;
public interface IParameterPrecheckAttribute
{
    public void RunParameterPrecheck(ParameterPrecheckContext context);
}
