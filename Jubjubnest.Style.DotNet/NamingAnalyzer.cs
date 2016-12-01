using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Analyzes the naming rules.
	/// </summary>
	[ DiagnosticAnalyzer( LanguageNames.CSharp ) ]
	public class NamingAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>Types must be named with PascalCasing.</summary>
		public static RuleDescription NameTypesWithPascalCasing { get; } =
				new RuleDescription( nameof( NameTypesWithPascalCasing ), "Naming" );

		/// <summary>Methods must be named with PascalCasing.</summary>
		public static RuleDescription NameMethodsWithPascalCasing { get; } =
				new RuleDescription( nameof( NameMethodsWithPascalCasing ), "Naming" );

		/// <summary>Namespaces must be named with PascalCasing.</summary>
		public static RuleDescription NameNamespacesWithPascalCasing { get; } =
				new RuleDescription( nameof( NameNamespacesWithPascalCasing ), "Naming" );

		/// <summary>Variables must be named with camelCase.</summary>
		public static RuleDescription NameVariablesWithCamelCase { get; } =
				new RuleDescription( nameof( NameVariablesWithCamelCase ), "Naming" );

		/// <summary>Properties must be named with PascalCase.</summary>
		public static RuleDescription NamePropertiesWithPascalCase { get; } =
				new RuleDescription( nameof( NamePropertiesWithPascalCase ), "Naming" );

		/// <summary>Events must be named with PascalCase.</summary>
		public static RuleDescription NameEventsWithPascalCase { get; } =
				new RuleDescription( nameof( NameEventsWithPascalCase ), "Naming" );

		/// <summary>Fields must be named with camelCase.</summary>
		public static RuleDescription NameFieldsWithCamelCase { get; } =
				new RuleDescription( nameof( NameFieldsWithCamelCase ), "Naming" );

		/// <summary>Constants must be named with CAPITAL_CASE.</summary>
		public static RuleDescription NameConstantsWithCapitalCase { get; } =
				new RuleDescription( nameof( NameConstantsWithCapitalCase ), "Naming" );

		/// <summary>Enum values must be named with PascalCase.</summary>
		public static RuleDescription NameEnumValuesWithPascalCase { get; } =
				new RuleDescription( nameof( NameEnumValuesWithPascalCase ), "Naming" );

		/// <summary>Exceptions must end with Exception.</summary>
		public static RuleDescription NameExceptionsWithExceptionSuffix { get; } =
				new RuleDescription( nameof( NameExceptionsWithExceptionSuffix ), "Naming" );

		/// <summary>Interfaces must start with I.</summary>
		public static RuleDescription NameInterfacesWithIPrefix { get; } =
				new RuleDescription( nameof( NameInterfacesWithIPrefix ), "Naming" );

		/// <summary>Type parameters must start with T.</summary>
		public static RuleDescription NameTypeParameterWithTPrefix { get; } =
				new RuleDescription( nameof( NameTypeParameterWithTPrefix ), "Naming" );

		/// <summary>Type parameters must have better name than just 'T'.</summary>
		public static RuleDescription NameTypeParameterWithDescriptiveName { get; } =
				new RuleDescription( nameof( NameTypeParameterWithDescriptiveName ), "Naming" );

		/// <summary>
		/// Supported diagnostic rules.
		/// </summary>
		public override ImmutableArray< DiagnosticDescriptor > SupportedDiagnostics =>
				ImmutableArray.Create(
					NameTypesWithPascalCasing.Rule,
					NameMethodsWithPascalCasing.Rule,
					NameNamespacesWithPascalCasing.Rule,
					NameVariablesWithCamelCase.Rule,
					NamePropertiesWithPascalCase.Rule,
					NameFieldsWithCamelCase.Rule,
					NameConstantsWithCapitalCase.Rule,
					NameEnumValuesWithPascalCase.Rule,
					NameExceptionsWithExceptionSuffix.Rule,
					NameInterfacesWithIPrefix.Rule );

		/// <summary>
		/// Initialize the analyzer.
		/// </summary>
		/// <param name="context">Analysis context the analysis actions are registered on.</param>
		public override void Initialize( AnalysisContext context )
		{
			// Register actions.
			context.RegisterSymbolAction( AnalyzeTypeName, SymbolKind.NamedType );
			context.RegisterSymbolAction( AnalyzeFieldName, SymbolKind.Field );
			context.RegisterSymbolAction( AnalyzePropertyName, SymbolKind.Property );
			context.RegisterSymbolAction( AnalyzeMethodName, SymbolKind.Method );
			context.RegisterSymbolAction( AnalyzeNamespaceName, SymbolKind.Namespace );
			context.RegisterSymbolAction( AnalyzeEventName, SymbolKind.Event );

			// Stuff.
			context.RegisterSyntaxNodeAction( AnalyzeEnumValues, SyntaxKind.EnumMemberDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeVariableNames, SyntaxKind.VariableDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeParameterNames, SyntaxKind.MethodDeclaration );
		}

		/// <summary>
		/// Analyze enumeration values.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeEnumValues( SyntaxNodeAnalysisContext context )
		{
			// Get the enum name.
			var enumSyntax = ( EnumMemberDeclarationSyntax )context.Node;
			var enumName = enumSyntax.Identifier.ToString();

			// Ensure the naming is correct.
			if( !IsPascalCase( enumName ) )
			{
				// Naming not correct. Report.
				var diagnostic = Diagnostic.Create(
						NameEnumValuesWithPascalCase.Rule,
						enumSyntax.Identifier.GetLocation(),
						enumName );
				context.ReportDiagnostic( diagnostic );
			}
		}

		/// <summary>
		/// Analyze parameter names.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeParameterNames( SyntaxNodeAnalysisContext context )
		{
			// Check the names.
			var method = (MethodDeclarationSyntax)context.Node;
			foreach( var parameter in method.ParameterList.Parameters )
				CheckName( context, parameter.Identifier, NameVariablesWithCamelCase, IsCamelCase );

			// If there are no type parameters, we're done now.
			// This prevents null-reference exception in the foreach.
			if( method.TypeParameterList == null )
				return;

			// Type parameters. Go through all.
			foreach( var typeParameter in method.TypeParameterList.Parameters )
			{
				// Check the naming on the type parameters.
				CheckName(
						context, typeParameter.Identifier, NameTypesWithPascalCasing,
						IsPascalCase, "type parameter" );
				CheckPrefix(
						"T", typeParameter.Identifier, NameTypeParameterWithTPrefix,
						IsPascalCase, context.ReportDiagnostic );
			}
		}

		/// <summary>
		/// Analyze variable names.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeVariableNames( SyntaxNodeAnalysisContext context )
		{
			// Check variables.
			var variableSyntax = (VariableDeclarationSyntax)context.Node;
			foreach( var variable in variableSyntax.Variables )
				CheckName( context, variable.Identifier, NameVariablesWithCamelCase, IsCamelCase );
		}

		/// <summary>
		/// Analyze the type names.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeTypeName( SymbolAnalysisContext context )
		{
			// Get the syntax bits referring to this symbol.
			var syntaxRefs =
					context.Symbol.DeclaringSyntaxReferences
						.Select( r => r.GetSyntax( context.CancellationToken ) )
						.ToList();

			// Check the naming.
			CheckName(
					context, NameTypesWithPascalCasing, IsPascalCase,
					SyntaxHelper.GetItemType( syntaxRefs.First() ) );

			// Check the type-specific rules.
			foreach( var syntax in syntaxRefs )
			{
				// Check for class rules.
				if( syntax.IsKind( SyntaxKind.ClassDeclaration ) )
				{
					// Class. Check exception naming.
					var classSyntax = (ClassDeclarationSyntax)syntax;
					CheckExceptionName( context, classSyntax );
				}

				// Check for interface rules.
				if( syntax.IsKind( SyntaxKind.InterfaceDeclaration ) )
				{
					// Interface. Save for I prefix.
					var interfaceSyntax = ( InterfaceDeclarationSyntax )syntax;
					CheckPrefix(
							"I", interfaceSyntax.Identifier, NameInterfacesWithIPrefix,
							IsPascalCase, context.ReportDiagnostic );
				}
			}
		}

		/// <summary>
		/// Check exception anme rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="classSyntax">Class declaration syntax.</param>
		private static void CheckExceptionName(
			SymbolAnalysisContext context,
			ClassDeclarationSyntax classSyntax )
		{
			// Make sure this is an exception class.
			if( classSyntax.BaseList == null ||
				!classSyntax.BaseList.Types.Any( bt => bt.ToString().EndsWith( "Exception" ) ) )
			{
				// Not an exception class. Stop processing.
				return;
			}

			// This is an exception class. If it ends in 'Exception' everything is okay.
			if( context.Symbol.Name.EndsWith( "Exception" ) )
				return;

			// Doesn't end in 'Exception'.
			// Report a diagnostic for each location.
			foreach( var l in context.Symbol.Locations )
			{
				// Report.
				var diagnostic = Diagnostic.Create(
						NameExceptionsWithExceptionSuffix.Rule,
						l, context.Symbol.Name );
				context.ReportDiagnostic( diagnostic );
			}
		}

		/// <summary>
		/// Check for prefixes.
		/// </summary>
		/// <param name="prefix">Expected prefix.</param>
		/// <param name="token">Token to check.</param>
		/// <param name="rule">Rule descriptor.</param>
		/// <param name="predicate">Naming rule for the remaining bits.</param>
		/// <param name="report">Reporting function.</param>
		private static void CheckPrefix(
			string prefix,
			SyntaxToken token,
			RuleDescription rule,
			Func< string, bool > predicate,
			Action< Diagnostic > report )
		{
			// Get the name.
			var name = token.ToString();

			// Chekc for prefix rules.
			if( name.StartsWith( prefix ) &&
				name.Length > prefix.Length &&
				predicate( name.Substring( prefix.Length, 1 ) ) )
			{
				// Prefix in order. Stop processing.
				return;
			}

			// Prefix faulty. Report warning.
			var diagnostic = Diagnostic.Create(
					rule.Rule,
					token.GetLocation(), name );
			report( diagnostic );
		}

		/// <summary>
		/// Check field naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeFieldName( SymbolAnalysisContext context )
		{
			// Delegate.
			CheckName( context, NameFieldsWithCamelCase, IsCamelCase );
		}

		/// <summary>
		/// Check property naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzePropertyName( SymbolAnalysisContext context )
		{
			// Delegate.
			CheckName( context, NamePropertiesWithPascalCase, IsPascalCase );
		}

		/// <summary>
		/// Check method naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeMethodName( SymbolAnalysisContext context )
		{
			// Delegate.
			CheckName( context, NameMethodsWithPascalCasing, IsPascalCase );
		}

		/// <summary>
		/// Check namespace naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeNamespaceName( SymbolAnalysisContext context )
		{
			// Delegate.
			CheckName( context, NameNamespacesWithPascalCasing, IsPascalCase );
		}

		/// <summary>
		/// Check event naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeEventName( SymbolAnalysisContext context )
		{
			// Delegate.
			CheckName( context, NameEventsWithPascalCase, IsPascalCase );
		}

		/// <summary>
		/// Check naming rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="ruleDescription">Rule to check for.</param>
		/// <param name="predicate">Name condition.</param>
		/// <param name="args">Format args.</param>
		private static void CheckName(
			SymbolAnalysisContext context,
			RuleDescription ruleDescription,
			Func< string, bool > predicate,
			params object[] args )
		{
			// If this is implicit symbol, skip it for naming checks.
			if( context.Symbol.IsImplicitlyDeclared )
				return;

			// If this can't be referenced by name (property getters/setters)
			// we don't check for naming rules.
			if( ! context.Symbol.CanBeReferencedByName )
				return;

			// Check for predicate.
			if( !predicate( context.Symbol.Name ) )
			{
				// Naming rules not in order.

				// Report each location.
				foreach( var location in context.Symbol.Locations )
				{
					// Create diagnostic and report.
					var formatParams = new List< object >( args ) { context.Symbol.Name };
					var diagnostic = Diagnostic.Create(
							ruleDescription.Rule,
							location,
							formatParams.ToArray() );
					context.ReportDiagnostic( diagnostic );
				}
			}
		}

		/// <summary>
		/// Check naming for syntax token.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="identifier">Token identifier.</param>
		/// <param name="ruleDescription">Rule to check for.</param>
		/// <param name="predicate">Name condition.</param>
		/// <param name="args">Format args.</param>
		private static void CheckName(
			SyntaxNodeAnalysisContext context,
			SyntaxToken identifier,
			RuleDescription ruleDescription,
			Func< string, bool > predicate,
			params object[] args )
		{
			// Check the name.
			var name = identifier.ToString();
			if( !predicate( name ) )
			{
				// Name is faulty. Report.
				var formatParams = new List< object >( args ) { name };
				var diagnostic = Diagnostic.Create(
						ruleDescription.Rule,
						identifier.GetLocation(),
						formatParams.ToArray() );
				context.ReportDiagnostic( diagnostic );
			}
		}

		/// <summary>
		/// Regex for checking the pascal case names.
		/// </summary>
		private static readonly Regex pascalCase = new Regex( @"
			^(?>
				[A-Z]			# Normal pascal casing
				[a-z]+
			|
				[0-9]+			# Numbering.
			|
				[A-Z][A-Z]?		# Abbreviation
				(?>				# Abbreviation must be followed by normal pascal casing or end of name.
					[A-Z][a-z]+
				|
					$
				)
			)+$
		", RegexOptions.IgnorePatternWhitespace );

		/// <summary>
		/// Method for checking pascal casing.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>True, if name is pascal cased.</returns>
		private static bool IsPascalCase( string name )
		{
			// Check with regex.
			return pascalCase.IsMatch( name );
		}

		/// <summary>
		/// Camel case naming rule.
		/// </summary>
		private static readonly Regex camelCase = new Regex( @"^[a-z]+(?>[A-Z][A-Z]?[a-z]+|[0-9])*[A-Z]?$" );

		/// <summary>
		/// Method for checking camel casing.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>True, if name is camel cased.</returns>
		private static bool IsCamelCase( string name )
		{
			// Check with regex.
			return camelCase.IsMatch( name );
		}

		/// <summary>
		/// CAPITAL_CASE naming rule.
		/// </summary>
		private static readonly Regex capitalCase = new Regex( @"^[A-Z]+(?>_[A-Z]+|_[0-9]+)*$" );

		/// <summary>
		/// Method for checking capital casing.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>True, if name is capital cased.</returns>
		private static bool IsCapitalCase( string name )
		{
			// Check with regex.
			return capitalCase.IsMatch( name );
		}
	}
}
