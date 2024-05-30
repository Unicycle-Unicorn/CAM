using Microsoft.Extensions.Logging;

namespace AuthProvider.RuntimePrecheck.Context;

public abstract class AbstractPrecheckContext
{
    public List<RuntimePrecheckIssue> Issues = [];

    public string Name;
    public AbstractPrecheckContext(string name)
    {
        Name = name;
    }

    public abstract void RunPrecheck();

    private void AddIssue(RuntimePrecheckIssue issue) => Issues.Add(issue);

    public void AddFatal(string issue, string? suggestion = null) => AddIssue(new(Severity.FATAL, issue, suggestion));
    public void AddWarning(string issue, string? suggestion = null) => AddIssue(new(Severity.WARNING, issue, suggestion));
    public void AddSuggestion(string issue, string? suggestion = null) => AddIssue(new(Severity.SUGGESTION, issue, suggestion));

    public void PrettyPrintIssues(ILogger logger)
    {
        foreach (var issue in Issues)
        {
            issue.PrettyPrint(logger);
        }
    }

    public bool ShouldError()
    {
        foreach (var issue in Issues)
        {
            if (issue.Severity == Severity.FATAL)
            {
                return true;
            }
        }

        return false;
    }
}
