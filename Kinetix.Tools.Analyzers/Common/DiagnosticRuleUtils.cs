using Microsoft.CodeAnalysis;

namespace Kinetix.Tools.Analyzers.Common
{
    public static class DiagnosticRuleUtils
    {
        public static DiagnosticDescriptor CreateRule(string id, string title, string messageFormat, string category, string description, DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: true,
                description: description);
        }
    }
}
