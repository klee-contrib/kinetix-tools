using System;
using System.Collections.Immutable;
using System.Linq;
using Kinetix.Tools.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kinetix.Tools.Analyzers.Diagnostics.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KTA1104_AsyncMethodShouldHaveCancellationToken : DiagnosticAnalyzer
{
    public const string DiagnosticId = "KTA1104";
    private const string Category = "Design";
    private static readonly string Description = "La méthode asynchrone ne prend pas de CancellationToken en dernier paramètre.";
    private static readonly string MessageFormat = "La méthode asynchrone ne prend pas de CancellationToken en dernier paramètre";
    private static readonly string Title = "Toute méthode asynchrone doit prendre un CancellationToken en dernier paramètre";

    private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description, DiagnosticSeverity.Info);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Méthode d'initialisation de l'analyseur.
    /// </summary>
    /// <param name="context">Le contexte.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSymbolAction(CheckForCancellableInvocation, SymbolKind.Method);
    }

    /// <summary>
    /// Trouve des diagnostics sur un champ.
    /// </summary>
    /// <param name="context">Le contexte.</param>
    private void CheckForCancellableInvocation(SymbolAnalysisContext context)
    {
        // On récupère les informations nécessaires du contexte du symbole.
        var location = context.Symbol.Locations.First();
        var root = location.SourceTree?.GetRoot();

        if (root?.FindNode(location.SourceSpan) is not MethodDeclarationSyntax method)
        {
            return;
        }

        var semanticModel = context.Compilation.GetSemanticModel(location.SourceTree!);

        var cancellationTokenType = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken")!;
        var genericTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        var genericValueTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1")!;

        var semMethod = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

        // Si la méthode a déjà un token en paramètre, c'est bon.
        if (semMethod?.Parameters.Any(p => p.Type.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability)) ?? true)
        {
            return;
        }

        // On récupère le corps de la méthode.
        if (method?.ChildNodes().FirstOrDefault(nœud => nœud as BlockSyntax != null) is not BlockSyntax body)
        {
            return;
        }

        var hasCancellableInvocation = false;

        foreach (var calledMethod in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (semanticModel.GetOperation(calledMethod) is IInvocationOperation calledInvocation)
            {
                if (InvocationMethodTakesAToken(calledInvocation.TargetMethod, calledInvocation.Arguments, cancellationTokenType))
                {
                    hasCancellableInvocation = true;
                    break;
                }
                else if (MethodHasCancellationTokenOverload(context.Compilation, calledInvocation.TargetMethod, cancellationTokenType, genericTask, genericValueTask))
                {
                    hasCancellableInvocation = true;
                    break;
                }
            }
        }

        if (hasCancellableInvocation)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0]));
        }
    }

    private bool InvocationMethodTakesAToken(IMethodSymbol method, ImmutableArray<IArgumentOperation> arguments, INamedTypeSymbol cancellationTokenType)
    {
        return
            method.Parameters is [.., IParameterSymbol lastParameter] &&
            (InvocationIgnoresOptionalCancellationToken(lastParameter, arguments, cancellationTokenType) ||
            InvocationIsUsingParamsCancellationToken(lastParameter, arguments, cancellationTokenType));
    }

    private bool InvocationIgnoresOptionalCancellationToken(IParameterSymbol lastParameter, ImmutableArray<IArgumentOperation> arguments, INamedTypeSymbol cancellationTokenType)
    {
        if (lastParameter.Type.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability) &&
            lastParameter.IsOptional)
        {
            return AnyArgument(
                arguments,
                static (a, cancellationTokenType) => a.Parameter != null && a.Parameter.Type.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability) && a.ArgumentKind == ArgumentKind.DefaultValue,
                cancellationTokenType);
        }

        return false;
    }

    private bool InvocationIsUsingParamsCancellationToken(IParameterSymbol lastParameter, ImmutableArray<IArgumentOperation> arguments, INamedTypeSymbol cancellationTokenType)
    {
        if (lastParameter.IsParams &&
               lastParameter.Type is IArrayTypeSymbol arrayTypeSymbol &&
               arrayTypeSymbol.ElementType.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability))
        {
            var paramsArgument = arguments.FirstOrDefault(a => a.ArgumentKind == ArgumentKind.ParamArray);
            if (paramsArgument?.Value is IArrayCreationOperation arrayOperation)
            {
                return arrayOperation.Initializer!.ElementValues.IsEmpty;
            }
        }

        return false;
    }

    private static bool AnyArgument<TArg>(ImmutableArray<IArgumentOperation> arguments, Func<IArgumentOperation, TArg, bool> predicate, TArg arg)
    {
        for (int i = arguments.Length - 1; i >= 0; i--)
        {
            if (predicate(arguments[i], arg))
            {
                return true;
            }
        }

        return false;
    }

    private bool MethodHasCancellationTokenOverload(Compilation compilation, IMethodSymbol method, INamedTypeSymbol cancellationTokenType, INamedTypeSymbol genericTask, INamedTypeSymbol genericValueTask)
    {
        var overload = method.ContainingType
            .GetMembers(method.Name)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(methodToCompare => HasSameParametersPlusCancellationToken(compilation, method, methodToCompare, cancellationTokenType, genericTask, genericValueTask));

        return overload != null;

        // Checks if the parameters of the two passed methods only differ in a ct.
        bool HasSameParametersPlusCancellationToken(
            Compilation compilation,
            IMethodSymbol originalMethod,
            IMethodSymbol methodToCompare,
            INamedTypeSymbol cancellationTokenType,
            INamedTypeSymbol genericTask,
            INamedTypeSymbol genericValueTask)
        {
            // Avoid comparing to itself, or when there are no parameters, or when the last parameter is not a ct
            if (originalMethod.Equals(methodToCompare, SymbolEqualityComparer.IncludeNullability) ||
                methodToCompare.Parameters.Count(p => p.Type.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability)) != 1 ||
                !methodToCompare.Parameters[^1].Type.Equals(cancellationTokenType, SymbolEqualityComparer.IncludeNullability))
            {
                return false;
            }

            var originalMethodWithAllParameters = (originalMethod.ReducedFrom ?? originalMethod).OriginalDefinition;
            var methodToCompareWithAllParameters = (methodToCompare.ReducedFrom ?? methodToCompare).OriginalDefinition;

            // Ensure parameters only differ by one - the ct
            if (originalMethodWithAllParameters.Parameters.Length != methodToCompareWithAllParameters.Parameters.Length - 1)
            {
                return false;
            }

            // Now compare the types of all parameters before the ct
            // The largest i is the number of parameters in the method that has fewer parameters
            for (int i = 0; i < originalMethodWithAllParameters.Parameters.Length; i++)
            {
                var originalParameter = originalMethodWithAllParameters.Parameters[i];
                var comparedParameter = methodToCompareWithAllParameters.Parameters[i];
                if (!originalParameter.Type.Equals(comparedParameter.Type, SymbolEqualityComparer.IncludeNullability))
                {
                    return false;
                }
            }

            // Overload is  valid if its return type is implicitly convertable
            var toCompareReturnType = methodToCompareWithAllParameters.ReturnType;
            var originalReturnType = originalMethodWithAllParameters.ReturnType;
            if (!IsAssignableTo(toCompareReturnType, originalReturnType, compilation))
            {
                // Generic Task-like types are special since awaiting them essentially erases the task-like type.
                // If both types are Task-like we will warn if their generic arguments are convertable to each other.
                if (IsTaskLikeType(originalReturnType) && IsTaskLikeType(toCompareReturnType) &&
                    originalReturnType is INamedTypeSymbol originalNamedType &&
                    toCompareReturnType is INamedTypeSymbol toCompareNamedType &&
                    TypeArgumentsAreConvertable(originalNamedType, toCompareNamedType))
                {
                    return true;
                }

                return false;
            }

            return true;

            bool IsTaskLikeType(ITypeSymbol typeSymbol)
            {
                if (genericTask is not null &&
                    typeSymbol.OriginalDefinition.Equals(genericTask, SymbolEqualityComparer.IncludeNullability))
                {
                    return true;
                }

                if (genericValueTask is not null &&
                    typeSymbol.OriginalDefinition.Equals(genericValueTask, SymbolEqualityComparer.IncludeNullability))
                {
                    return true;
                }

                return false;
            }

            bool TypeArgumentsAreConvertable(INamedTypeSymbol left, INamedTypeSymbol right)
            {
                if (left.Arity != 1 ||
                    right.Arity != 1 ||
                    left.Arity != right.Arity)
                {
                    return false;
                }

                var leftTypeArgument = left.TypeArguments[0];
                var rightTypeArgument = right.TypeArguments[0];
                if (!IsAssignableTo(leftTypeArgument, rightTypeArgument, compilation))
                {
                    return false;
                }

                return true;
            }
        }
    }

    public static bool IsAssignableTo(ITypeSymbol fromSymbol, ITypeSymbol toSymbol, Compilation compilation)
        => fromSymbol != null && toSymbol != null && compilation.ClassifyCommonConversion(fromSymbol, toSymbol).IsImplicit;
}