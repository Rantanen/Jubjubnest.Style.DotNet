using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Text.RegularExpressions;

namespace MFiles.Style.DotNet
{
	[DiagnosticAnalyzer( LanguageNames.CSharp )]
	public class DocumentationAnalyzer : DiagnosticAnalyzer
	{
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly RuleDescription XmlDocumentEverythingWithSummary = new RuleDescription( nameof( XmlDocumentEverythingWithSummary ), "Documentation" );
        public static readonly RuleDescription XmlDocumentAllMethodParams = new RuleDescription( nameof( XmlDocumentAllMethodParams ), "Documentation" );
        public static readonly RuleDescription XmlDocumentReturnValues = new RuleDescription( nameof( XmlDocumentReturnValues ), "Documentation" );
        public static readonly RuleDescription XmlDocumentationNoMismatchedParam = new RuleDescription( nameof( XmlDocumentationNoMismatchedParam ), "Documentation" );
        public static readonly RuleDescription XmlDocumentNoEmptyContent = new RuleDescription( nameof( XmlDocumentNoEmptyContent ), "Documentation" );

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create(
					XmlDocumentEverythingWithSummary.Rule,
					XmlDocumentAllMethodParams.Rule,
					XmlDocumentReturnValues.Rule,
					XmlDocumentationNoMismatchedParam.Rule,
					XmlDocumentNoEmptyContent.Rule );

		public override void Initialize( AnalysisContext context )
		{
			context.RegisterSyntaxNodeAction( RequireDocumentation,
					SyntaxKind.InterfaceDeclaration,
					SyntaxKind.ClassDeclaration,
					SyntaxKind.StructDeclaration,
					SyntaxKind.EnumDeclaration,
					SyntaxKind.MethodDeclaration,
					SyntaxKind.PropertyDeclaration,
					SyntaxKind.FieldDeclaration,
					SyntaxKind.EnumMemberDeclaration );
		}

		private static void RequireDocumentation( SyntaxNodeAnalysisContext context )
		{
			var documentationTrivia = context.Node.GetLeadingTrivia()
					.Where( trivia =>
						trivia.IsKind( SyntaxKind.SingleLineDocumentationCommentTrivia ) ||
						trivia.IsKind( SyntaxKind.MultiLineDocumentationCommentTrivia ) )
					.Select( trivia => trivia.GetStructure() )
					.OfType<DocumentationCommentTriviaSyntax>()
					.SingleOrDefault();

			if( documentationTrivia == null )
			{
				// Create the diagnostic message and report it.
				var identifier = SyntaxHelper.GetIdentifier( context.Node );
				var diagnostic = Diagnostic.Create(
						XmlDocumentEverythingWithSummary.Rule,
						identifier.GetLocation(),
						SyntaxHelper.GetItemType( context.Node ), identifier.ToString() );
                context.ReportDiagnostic( diagnostic );
				return;
			}

			var xmlElements = documentationTrivia
					.ChildNodes()
					.OfType<XmlElementSyntax>()
					.ToList();

			var summaries = xmlElements.Where( xml => xml.StartTag.Name.ToString() == "summary" );

			if( ! summaries.Any() )
			{
				// Create the diagnostic message and report it.
				var identifier = SyntaxHelper.GetIdentifier( context.Node );
				var diagnostic = Diagnostic.Create(
						XmlDocumentEverythingWithSummary.Rule,
						identifier.GetLocation(),
						SyntaxHelper.GetItemType( context.Node ), identifier.ToString() );
                context.ReportDiagnostic( diagnostic );
			}

			if( context.Node.IsKind( SyntaxKind.MethodDeclaration ) )
			{
				var method = ( MethodDeclarationSyntax )context.Node;

				var paramElements = xmlElements.Where( xml =>
						xml.StartTag.Name.ToString() == "param" ).ToList();
				Dictionary<string, string> paramDocs = new Dictionary<string, string>();

				var paramNodes = method.ParameterList.Parameters.ToDictionary( p => p.Identifier.ToString() );

				foreach( var paramElement in paramElements )
				{
					var nameAttribute = paramElement.StartTag.Attributes
							.OfType<XmlNameAttributeSyntax>()
							.Single();

					var paramName = nameAttribute.Identifier.ToString();
					if( ! paramNodes.ContainsKey( paramName ) )
					{
						// Create the diagnostic message and report it.
						var identifier = SyntaxHelper.GetIdentifier( context.Node );
						var diagnostic = Diagnostic.Create(
								XmlDocumentationNoMismatchedParam.Rule,
								nameAttribute.GetLocation(),
								paramName );
						context.ReportDiagnostic( diagnostic );
						continue;
					}

					paramDocs.Add( nameAttribute.Identifier.ToString(), paramElement.Content.ToString() );
				}

				foreach( var paramPair in paramNodes )
				{
					var paramName = paramPair.Key;
					if( ! paramDocs.ContainsKey( paramName ) )
					{
						// Create the diagnostic message and report it.
						var identifier = SyntaxHelper.GetIdentifier( context.Node );
						var diagnostic = Diagnostic.Create(
								XmlDocumentAllMethodParams.Rule,
								paramPair.Value.Identifier.GetLocation(),
								paramPair.Key );
						context.ReportDiagnostic( diagnostic );
					}
				}

				if( method.ReturnType.ToString() != "void" &&
					! xmlElements.Any( xml => xml.StartTag.Name.ToString() == "returns" ) )
				{
					// Create the diagnostic message and report it.
					var identifier = SyntaxHelper.GetIdentifier( context.Node );
					var diagnostic = Diagnostic.Create(
							XmlDocumentReturnValues.Rule,
							method.Identifier.GetLocation(),
							method.Identifier.ToString() );
					context.ReportDiagnostic( diagnostic );
				}
			}

			foreach( var element in xmlElements )
				EnsureNonEmptyContent( context, element );
        }

		private static void EnsureNonEmptyContent( SyntaxNodeAnalysisContext context, XmlElementSyntax element )
		{
			if( element.Content.ToString() == "" )
			{
				// Create the diagnostic message and report it.
				var identifier = SyntaxHelper.GetIdentifier( context.Node );
				var diagnostic = Diagnostic.Create(
						XmlDocumentNoEmptyContent.Rule,
						element.GetLocation(),
						element.StartTag.Name.ToString() );
				context.ReportDiagnostic( diagnostic );
			}
		}

    }
}
