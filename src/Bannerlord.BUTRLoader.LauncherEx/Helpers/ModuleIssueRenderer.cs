using Bannerlord.BUTRLoader.Localization;
using Bannerlord.ModuleManager;

using System;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleIssueRenderer
    {
        public static string Render(ModuleIssue issue) => RenderTextObject(issue).ToString();

        public static BUTRTextObject RenderTextObject(ModuleIssue issue) => issue.Type switch
        {
            ModuleIssueType.Missing => new BUTRTextObject("{=J3Uh6MV4}Missing '{ID}' {VERSION} in modules list")
                .SetTextVariable("ID", issue.SourceId)
                .SetTextVariable("VERSION", issue.SourceVersion.Min.ToString()),

            ModuleIssueType.MissingDependencies => new BUTRTextObject("{=3eQSr6wt}Missing '{ID}' {VERSION}")
                .SetTextVariable("ID", issue.Target.Id)
                .SetTextVariable("VERSION", issue.SourceVersion == ApplicationVersionRange.Empty
                    ? issue.SourceVersion.ToString()
                    : issue.SourceVersion.Min == issue.SourceVersion.Max
                        ? issue.SourceVersion.Min.ToString()
                        : ""),
            ModuleIssueType.DependencyMissingDependencies => new BUTRTextObject("{=U858vdQX}'{ID}' is missing it's dependencies!")
                .SetTextVariable("ID", issue.SourceId),

            ModuleIssueType.DependencyValidationError => new BUTRTextObject("{=1LS8Z5DU}'{ID}' has unresolved issues!")
                .SetTextVariable("ID", issue.SourceId),

            ModuleIssueType.VersionMismatchLessThanOrEqual => new BUTRTextObject("{=Vjz9HQ41}'{ID}' wrong version <= {VERSION}")
                .SetTextVariable("ID", issue.SourceId)
                .SetTextVariable("VERSION", issue.SourceVersion.ToString()),
            ModuleIssueType.VersionMismatchLessThan => new BUTRTextObject("{=ZvnlL7VE}'{ID}' wrong version < [{VERSION}]")
                .SetTextVariable("ID", issue.SourceId)
                .SetTextVariable("VERSION", issue.SourceVersion.ToString()),
            ModuleIssueType.VersionMismatchGreaterThan => new BUTRTextObject("{=EfNuH2bG}'{ID}' wrong version > [{VERSION}]")
                .SetTextVariable("ID", issue.SourceId)
                .SetTextVariable("VERSION", issue.SourceVersion.ToString()),

            ModuleIssueType.Incompatible => new BUTRTextObject("{=zXDidmpQ}'{ID}' is incompatible with this module")
                .SetTextVariable("ID", issue.SourceId),

            ModuleIssueType.DependencyConflictDependentAndIncompatible => new BUTRTextObject("{=4KFwqKgG}Module '{ID}' is both depended upon and marked as incompatible")
                .SetTextVariable("ID", issue.SourceId),
            ModuleIssueType.DependencyConflictDependentLoadBeforeAndAfter => new BUTRTextObject("{=9DRB6yXv}Module '{ID}' is both depended upon as LoadBefore and LoadAfter")
                .SetTextVariable("ID", issue.SourceId),
            ModuleIssueType.DependencyConflictCircular => new BUTRTextObject("{=RC1V9BbP}Circular dependencies. '{TARGETID}' and '{SOURCEID}' depend on each other")
                .SetTextVariable("TARGETID", issue.Target.Id)
                .SetTextVariable("SOURCEID", issue.SourceId),

            ModuleIssueType.DependencyNotLoadedBeforeThis => new BUTRTextObject("{=s3xbuejE}'{TARGETID}' should be loaded before '{SOURCEID}'")
                .SetTextVariable("TARGETID", issue.Target.Id)
                .SetTextVariable("SOURCEID", issue.SourceId),

            ModuleIssueType.DependencyNotLoadedAfterThis => new BUTRTextObject("{=2ALJB7z2}'{TARGETID}' should be loaded after '{SOURCEID}'")
                .SetTextVariable("ID", issue.Target.Id),

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}