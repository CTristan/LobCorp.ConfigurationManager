// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using LobotomyCorporation.Mods.ConfigurationManager.Integration.Diagnostics;
using LobotomyCorporation.Mods.ConfigurationManager.Integration.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration
{
    /// <summary>
    /// Incremental source generator that emits runtime glue for optional-dependency
    /// integration with LobCorp.ConfigurationManager into the consuming mod's assembly.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class ConfigurationManagerGenerator : IIncrementalGenerator
    {
        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidateNodes = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => BindingScanner.IsBindCandidate(node),
                transform: static (ctx, _) =>
                    BindingScanner.TryExtract(
                        (InvocationExpressionSyntax)ctx.Node,
                        ctx.SemanticModel
                    )
            );

            var bindings = candidateNodes
                .Where(static model => model is not null)
                .Select(static (model, _) => model!)
                .Collect();

            var modAttribute = context
                .SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) => ModAttributeScanner.IsCandidate(node),
                    transform: static (ctx, _) =>
                        ModAttributeScanner.TryExtract((AttributeSyntax)ctx.Node, ctx.SemanticModel)
                )
                .Where(static model => model is not null)
                .Select(static (model, _) => model!)
                .Collect();

            var combined = bindings.Combine(modAttribute);

            context.RegisterPostInitializationOutput(static ctx =>
            {
                ctx.AddSource(
                    "ConfigurationManager.Api.g.cs",
                    SourceText.From(ApiSurfaceEmitter.Source, Encoding.UTF8)
                );
                ctx.AddSource(
                    "ConfigurationManager.CmAttributes.g.cs",
                    SourceText.From(CmAttributesEmitter.Source, Encoding.UTF8)
                );
            });

            context.RegisterSourceOutput(combined, EmitRegistration);
        }

        private static void EmitRegistration(
            SourceProductionContext spc,
            (ImmutableArray<BindingModel> Bindings, ImmutableArray<ModAttributeModel> ModAttrs) data
        )
        {
            if (data.ModAttrs.Length > 1)
            {
                spc.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.DuplicateModAttribute,
                        data.ModAttrs[0].Location
                    )
                );
            }

            var modAttribute = data.ModAttrs.Length == 0 ? null : data.ModAttrs[0];
            var uniqueBindings = DeduplicateAndReport(spc, data.Bindings);

            var registration = RegistrationEmitter.Emit(uniqueBindings, modAttribute);
            spc.AddSource(
                "ConfigurationManager.Registration.g.cs",
                SourceText.From(registration, Encoding.UTF8)
            );
        }

        private static ImmutableArray<BindingModel> DeduplicateAndReport(
            SourceProductionContext spc,
            ImmutableArray<BindingModel> scannedBindings
        )
        {
            var seen = new HashSet<string>();
            var uniqueBindings = new List<BindingModel>(scannedBindings.Length);
            foreach (var model in scannedBindings)
            {
                var identity = model.Section + "\0" + model.Key;
                if (!seen.Add(identity))
                {
                    spc.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateBinding,
                            model.Location,
                            model.Section,
                            model.Key
                        )
                    );
                    continue;
                }

                uniqueBindings.Add(model);
            }

            return [.. uniqueBindings];
        }
    }
}
