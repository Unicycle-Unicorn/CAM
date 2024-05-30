using Microsoft.Extensions.Logging;

namespace AuthProvider.RuntimePrecheck;
public class RuntimePrecheckIssue
{
    public string? SuggestedFix;
    public string Issue;
    public Severity Severity;

    public RuntimePrecheckIssue(Severity severity, string issue, string? suggestion)
    {
        Severity = severity;
        Issue = issue;
        SuggestedFix = suggestion;
    }

    public void PrettyPrint(ILogger logger)
    {
        string log = Issue;
        if (SuggestedFix != null)
        {
            log += $" ({SuggestedFix})";
        }
        switch (Severity)
        {
            case Severity.SUGGESTION:
                logger.LogInformation(log);
                break;
            case Severity.FATAL:
                logger.LogCritical(log);
                break;
            case Severity.WARNING:
                logger.LogWarning(log);
                break;
        }

    }
}

public enum Severity
{
    FATAL,
    WARNING,
    SUGGESTION
}
