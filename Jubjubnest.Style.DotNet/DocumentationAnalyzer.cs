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

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Analyzes the XML documentation.
	/// </summary>
	[ DiagnosticAnalyzer( LanguageNames.CSharp ) ]
	public class DocumentationAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>Require documentation on all elements.</summary>
		public static RuleDescription XmlDocumentEverythingWithSummary { get; } =
				new RuleDescription( nameof( XmlDocumentEverythingWithSummary ), "Documentation" );

		/// <summary>Require documentation on all method parameters.</summary>
		public static RuleDescription XmlDocumentAllMethodParams { get; } =
				new RuleDescription( nameof( XmlDocumentAllMethodParams ), "Documentation" );

		/// <summary>Require documentation on return values.</summary>
		public static RuleDescription XmlDocumentReturnValues { get; } =
				new RuleDescription( nameof( XmlDocumentReturnValues ), "Documentation" );

		/// <summary>Check that each documented parameter exists on the method.</summary>
		public static RuleDescription XmlDocumentationNoMismatchedParam { get; } =
				new RuleDescription( nameof( XmlDocumentationNoMismatchedParam ), "Documentation" );

		/// <summary>Check that the documentation XML elements are not empty.</summary>
		public static RuleDescription XmlDocumentationNoEmptyContent { get; } =
				new RuleDescription( nameof( XmlDocumentationNoEmptyContent ), "Documentation" );

		/// <summary>Check that the documentation XML elements are not empty.</summary>
		public static RuleDescription XmlNoMultipleXmlDocumentationSegments { get; } =
				new RuleDescription( nameof( XmlNoMultipleXmlDocumentationSegments ), "Documentation" );

		/// <summary>Check that the documentation XML elements are not empty.</summary>
		public static RuleDescription XmlNoMultipleParamsWithSameName { get; } =
				new RuleDescription( nameof( XmlNoMultipleParamsWithSameName ), "Documentation" );

		/// <summary>
		/// Supported diagnostic rules.
		/// </summary>
		public override ImmutableArray< DiagnosticDescriptor > SupportedDiagnostics =>
				ImmutableArray.Create(
					XmlDocumentEverythingWithSummary.Rule,
					XmlDocumentAllMethodParams.Rule,
					XmlDocumentReturnValues.Rule,
					XmlDocumentationNoMismatchedParam.Rule,
					XmlDocumentationNoEmptyContent.Rule,
					XmlNoMultipleXmlDocumentationSegments.Rule,
					XmlNoMultipleParamsWithSameName.Rule );

		/// <summary>
		/// Initialize the analyzer.
		/// </summary>
		/// <param name="context">Analysis context the analysis actions are registered on.</param>
		public override void Initialize( AnalysisContext context )
		{
			// Register the actions.
			context.RegisterSyntaxNodeAction( CheckXmlDocumentation,
					SyntaxKind.InterfaceDeclaration,
					SyntaxKind.ClassDeclaration,
					SyntaxKind.StructDeclaration,
					SyntaxKind.EnumDeclaration,
					SyntaxKind.MethodDeclaration,
					SyntaxKind.PropertyDeclaration,
					SyntaxKind.FieldDeclaration,
					SyntaxKind.EnumMemberDeclaration );
		}

		/// <summary>
		/// Check for the XML documentaiton.
		/// </summary>
		/// <param name="context"></param>
		private static void CheckXmlDocumentation( SyntaxNodeAnalysisContext context )
		{
			// Get all the documentaiton trivia.
			var documentationTrivias = context.Node.GetLeadingTrivia()
					.Where( trivia =>
						trivia.IsKind( SyntaxKind.SingleLineDocumentationCommentTrivia ) ||
						trivia.IsKind( SyntaxKind.MultiLineDocumentationCommentTrivia ) )
					.Select( trivia => trivia.GetStructure() )
					.OfType< DocumentationCommentTriviaSyntax >()
					.ToList();

			// Ensure there's only one doc-block at most.
			if( documentationTrivias.Count > 1 )
			{
				// Multiple blocks. Report and stop processing until the dev fixes this issue.

				// Remove the last trivia. We'll report only the preceding ones.
				documentationTrivias.RemoveAt( documentationTrivias.Count - 1 );

				// Report all preceding blocks.
				foreach( var trivia in documentationTrivias )
				{
					// Report.
					var diagnostic = Diagnostic.Create(
							XmlNoMultipleXmlDocumentationSegments.Rule,
							trivia.GetLocation() );
					context.ReportDiagnostic( diagnostic );
				}

				// Stop processing further.
				return;
			}

			// [Test], [TestCase] and [TestMethod] methods do not need XML documentation.
			// These methods should have a name that is descriptive enough.
			bool requiresDocumentation = true;
			if( context.Node.IsKind( SyntaxKind.MethodDeclaration ) )
			{
				// Check for the attributes on the method.
				var method = ( MethodDeclarationSyntax )context.Node;
				var attributes = method.AttributeLists.SelectMany( list => list.Attributes );

				// Check for the aforementioned attributes.
				var isTestMethod = attributes.Any( attr =>
				{
					// Compare the name.
					var attrName = attr.Name.ToString();
					return attrName == "Test" ||
							attrName == "TestCase" ||
							attrName == "TestMethod";
				} );

				// Set the require-value.
				requiresDocumentation = !isTestMethod;
			}

			// The remaining tests involve checking the documentation.
			// If there is no documentation and none is required we can stop here.
			if( !requiresDocumentation && documentationTrivias.Count == 0 )
				return;

			// Ensure the documentation exists.
			if( documentationTrivias.Count == 0 )
			{
				// No documentaiton.
				// Create the diagnostic message and report it.
				var identifier = SyntaxHelper.GetIdentifier( context.Node );
				var diagnostic = Diagnostic.Create(
						XmlDocumentEverythingWithSummary.Rule,
						identifier.GetLocation(),
						SyntaxHelper.GetItemType( context.Node ), identifier.ToString() );
				context.ReportDiagnostic( diagnostic );

				// Stop processing further here.
				// If there is no XML documentation tag, there's no real reason to
				// report missing parameters etc. either.
				return;
			}

			// Get the XML elements in the documentation.
			var xmlElements = documentationTrivias[ 0 ]
					.ChildNodes()
					.OfType< XmlElementSyntax >()
					.ToList();

			// Ensure a summary exists.
			var summaries = xmlElements.Where( xml => xml.StartTag.Name.ToString() == "summary" );
			if( ! summaries.Any() )
			{
				// No summary.
				// Create the diagnostic message and report it.
				var identifier = SyntaxHelper.GetIdentifier( context.Node );
				var diagnostic = Diagnostic.Create(
						XmlDocumentEverythingWithSummary.Rule,
						identifier.GetLocation(),
						SyntaxHelper.GetItemType( context.Node ), identifier.ToString() );
				context.ReportDiagnostic( diagnostic );
			}

			// If this is a method declaration, we'll need to check for "param" and "return" docs.
			if( context.Node.IsKind( SyntaxKind.MethodDeclaration ) )
			{
				// This is a method declaration. Check for additional XML elements.

				// Get the method and gather the method params by name.
				var method = ( MethodDeclarationSyntax )context.Node;
				var paramNodes = method.ParameterList.Parameters.ToDictionary( p => p.Identifier.ToString() );

				// Gather all <param> elements.
				var paramElements = xmlElements.Where( xml =>
						xml.StartTag.Name.ToString() == "param" ).ToList();

				// Go through all param XML elements.
				// Keep gathering the ones matching real parameters into a by-name dictionary while doing so.
				Dictionary< string, string > paramDocs = new Dictionary< string, string >();
				foreach( var paramElement in paramElements )
				{
					// Get the name for the element.
					var nameAttribute = paramElement.StartTag.Attributes
							.OfType< XmlNameAttributeSyntax >()
							.Single();

					// Check whether a parameter exists with that name.
					var paramName = nameAttribute.Identifier.ToString();
					if( ! paramNodes.ContainsKey( paramName ) )
					{
						// No parameter with the name found.
						// Create the diagnostic message and report it.
						var diagnostic = Diagnostic.Create(
								XmlDocumentationNoMismatchedParam.Rule,
								nameAttribute.GetLocation(),
								paramName );
						context.ReportDiagnostic( diagnostic );

						// Continue to the next element without adding this
						// one to the param docs dictionary as it doesn't
						// match an existing element.
						continue;
					}

					// Ensure there are no duplicate 'name' attributes.
					if( paramDocs.ContainsKey( nameAttribute.Identifier.ToString() ) )
					{
						// Duplicate attribute.
						// Create the diagnostic message and report it.
						var diagnostic = Diagnostic.Create(
								XmlNoMultipleParamsWithSameName.Rule,
								nameAttribute.GetLocation(),
								paramName );
						context.ReportDiagnostic( diagnostic );
						continue;
					}

					// Store the element by name in the dictionary.
					paramDocs.Add( nameAttribute.Identifier.ToString(), paramElement.Content.ToString() );
				}

				// Check all existing parameters against the <param> elements.
				foreach( var paramPair in paramNodes )
				{
					// Check if there is a <param> element for the parameter.
					var paramName = paramPair.Key;
					if( ! paramDocs.ContainsKey( paramName ) )
					{
						// No element exists.
						// Create the diagnostic message and report it.
						var diagnostic = Diagnostic.Create(
								XmlDocumentAllMethodParams.Rule,
								paramPair.Value.Identifier.GetLocation(),
								paramPair.Key );
						context.ReportDiagnostic( diagnostic );
					}
				}

				// If the method is non-void, ensure it has a <returns> element.
				if( method.ReturnType.ToString() != "void" &&
					xmlElements.All( xml => xml.StartTag.Name.ToString() != "returns" ) )
				{
					// Non-void without <returns>.
					// Create the diagnostic message and report it.
					var diagnostic = Diagnostic.Create(
							XmlDocumentReturnValues.Rule,
							method.Identifier.GetLocation(),
							method.Identifier.ToString() );
					context.ReportDiagnostic( diagnostic );
				}
			}

			// Ensure all XML elements have proper content.
			foreach( var element in xmlElements )
				EnsureNonEmptyContent( context, element );
		}

		/// <summary>
		/// Checks that the XML element has valid content.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="element">XML documentation element.</param>
		private static void EnsureNonEmptyContent(
			SyntaxNodeAnalysisContext context,
			XmlElementSyntax element )
		{
			// Check whether the content exists.
			if( string.IsNullOrWhiteSpace( element.Content.ToString() ) )
			{
				// Empty element.
				// Create the diagnostic message and report it.
				var diagnostic = Diagnostic.Create(
						XmlDocumentationNoEmptyContent.Rule,
						element.GetLocation(),
						element.StartTag.Name.ToString() );
				context.ReportDiagnostic( diagnostic );
			}
		}

	}
}
