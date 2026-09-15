using System;
using System.Collections.Generic;

public enum AuthoringCheckSeverity
{
    Info,
    Warning,
    Error
}

public sealed class AuthoringCheckResult
{
    public AuthoringCheckResult(
        string id,
        string category,
        AuthoringCheckSeverity severity,
        string title,
        string message,
        string targetPath = null,
        string fixLabel = null,
        Action fixAction = null)
    {
        Id = id;
        Category = category;
        Severity = severity;
        Title = title;
        Message = message;
        TargetPath = targetPath;
        FixLabel = fixLabel;
        FixAction = fixAction;
    }

    public string Id { get; }
    public string Category { get; }
    public AuthoringCheckSeverity Severity { get; }
    public string Title { get; }
    public string Message { get; }
    public string TargetPath { get; }
    public string FixLabel { get; }
    public Action FixAction { get; }
    public bool HasFix => FixAction != null && !string.IsNullOrWhiteSpace(FixLabel);

    public void ExecuteFix()
    {
        FixAction?.Invoke();
    }
}

public interface IAuthoringCheck
{
    void Run(List<AuthoringCheckResult> results);
}
