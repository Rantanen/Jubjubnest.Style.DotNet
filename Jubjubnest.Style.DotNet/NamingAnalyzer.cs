using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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

		/// <summary>Constants must be named with CamelCase.</summary>
		public static RuleDescription NameConstantsWithCamelCase { get; } =
				new RuleDescription( nameof( NameConstantsWithCamelCase ), "Naming" );

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

		/// <summary>Type parameters must have better name than just 'T'.</summary>
		public static RuleDescription NameFilesAccordingToTypeNames { get; } =
				new RuleDescription( nameof( NameFilesAccordingToTypeNames ), "Naming" );

		/// <summary>Type parameters must have better name than just 'T'.</summary>
		public static RuleDescription NameFoldersAccordingToNamespaces { get; } =
				new RuleDescription( nameof( NameFoldersAccordingToNamespaces ), "Naming" );

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
					NameEventsWithPascalCase.Rule,
					NameFieldsWithCamelCase.Rule,
					NameConstantsWithCamelCase.Rule,
					NameEnumValuesWithPascalCase.Rule,
					NameExceptionsWithExceptionSuffix.Rule,
					NameInterfacesWithIPrefix.Rule,
					NameFilesAccordingToTypeNames.Rule,
					NameFoldersAccordingToNamespaces.Rule );

		/// <summary>
		/// Initialize the analyzer.
		/// </summary>
		/// <param name="context">Analysis context the analysis actions are registered on.</param>
		public override void Initialize( AnalysisContext context )
		{
			// Ignore generated files.
			context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );

			// Register actions.
			context.RegisterSymbolAction( AnalyzeTypeName, SymbolKind.NamedType );
			context.RegisterSymbolAction( AnalyzePropertyName, SymbolKind.Property );
			context.RegisterSymbolAction( AnalyzeMethodName, SymbolKind.Method );
			context.RegisterSymbolAction( AnalyzeNamespaceName, SymbolKind.Namespace );

			// Stuff.
			context.RegisterSyntaxNodeAction( AnalyzeEnumValues, SyntaxKind.EnumMemberDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeVariableNames, SyntaxKind.VariableDeclarator );
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
			// Get the syntax node.
			var variable = (VariableDeclaratorSyntax)context.Node;

			// If this variable is a field, skip it here. We have separate checks for fields.
			FieldDeclarationSyntax field = variable.Parent.Parent as FieldDeclarationSyntax;
			EventFieldDeclarationSyntax eventField = variable.Parent.Parent as EventFieldDeclarationSyntax;
			if( field != null )
			{
				// Type field variable.

				// Check for modifiers.
				var isConst = field.Modifiers.Any( st => st.IsKind( SyntaxKind.ConstKeyword ) );
				var isReadOnly = field.Modifiers.Any( st => st.IsKind( SyntaxKind.ReadOnlyKeyword ) );

				// Delegate depending on the field type.
				if( isConst )
				{
					// Consts are named with capital case. These are true constants and immutable.
					CheckName( context, variable.Identifier, NameConstantsWithCamelCase, IsPascalCase );
				}
				else if( isReadOnly )
				{
					// While 'read only' is very close to a const, they are not immutable and as such behave
					// often much closer to normal variables instead of consts.
					CheckName( context, variable.Identifier, NameFieldsWithCamelCase, IsCamelCase );
				}
				else
				{
					// Normal variable.
					CheckName( context, variable.Identifier, NameFieldsWithCamelCase, IsCamelCase );
				}
			}
			else if( eventField != null )
			{
				// Events are considered public and are named with pascal case.
				CheckName( context, variable.Identifier, NameEventsWithPascalCase, IsPascalCase );
			}
			else
			{
				// Normal non-field variable.

				// Go through each variable in the declaration.
				CheckName( context, variable.Identifier, NameVariablesWithCamelCase, IsCamelCase );
			}
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

				// If this is a top-level type, we'll check for file naming.
				if( syntax.Parent.IsKind( SyntaxKind.NamespaceDeclaration ) )
				{
					// Resolve the file information.
					string file = syntax.SyntaxTree.FilePath;
					string filename = Path.GetFileNameWithoutExtension( file );

					// Get the location for the declaration for more specific target.
					// Try to find the identifier, otherwise use the whole block.
					Location declarationLocation = GetDeclarationLocation( syntax );

					// Check the file name matches the symbol name.
					if( context.Symbol.Name != filename )
					{
						// File name doesn't match the symbol. Report the issue.
						var diagnostic = Diagnostic.Create(
								NameFilesAccordingToTypeNames.Rule,
								declarationLocation,
								context.Symbol.Name,
								context.Symbol.Name + ".cs" );
						context.ReportDiagnostic( diagnostic );
					}
				}
			}
		}

		/// <summary>
		/// Gets the declaration location.
		/// </summary>
		/// <param name="syntax">The node from which the declaration should be found.</param>
		/// <returns>The location for declaration, or the whole node location.</returns>
		private static Location GetDeclarationLocation(
			SyntaxNode syntax
		)
		{
			// Go over all the types one by one. We get the identifier most reliably for them that way.
			Location declarationLocation = null;

			// Is class?
			ClassDeclarationSyntax declSyntax = syntax as ClassDeclarationSyntax;
			if( declSyntax != null )
				declarationLocation = declSyntax.Identifier.GetLocation();

			// Is interface?
			InterfaceDeclarationSyntax interfaceSyntax = syntax as InterfaceDeclarationSyntax;
			if( interfaceSyntax != null )
				declarationLocation = interfaceSyntax.Identifier.GetLocation();

			// Is enum?
			EnumDeclarationSyntax enumSyntax = syntax as EnumDeclarationSyntax;
			if( enumSyntax != null )
				declarationLocation = enumSyntax.Identifier.GetLocation();

			// Is something else?
			if( declarationLocation == null )
				declarationLocation = syntax.GetLocation();

			// Return the resolved location.
			return declarationLocation;
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
				!classSyntax.BaseList.Types.Any( bt => IsExceptionName( bt.ToString() ) ) )
			{

				// Not an exception class. Stop processing.
				return;
			}

			// This is an exception class. If it ends in 'Exception' everything is okay.
			if( IsExceptionName( context.Symbol.Name ) )
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
		/// Check whether the name is an exception name.
		/// </summary>
		/// <param name="typeName">Type name.</param>
		/// <returns>True, if the type name is an exception name.</returns>
		private static bool IsExceptionName( string typeName )
		{
			return typeName.EndsWith( "Exception", StringComparison.Ordinal );
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
			if( name.StartsWith( prefix, StringComparison.Ordinal ) &&
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
		private static readonly Regex PascalCaseRegex = new Regex( @"
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
			return PascalCaseRegex.IsMatch( name );
		}

		/// <summary>
		/// Camel case naming rule.
		/// </summary>
		private static readonly Regex CamelCaseRegex = new Regex( @"^[a-z]+(?>[A-Z][A-Z]?[a-z]+|[0-9])*[A-Z]?$" );

		/// <summary>
		/// Method for checking camel casing.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>True, if name is camel cased.</returns>
		private static bool IsCamelCase( string name )
		{
			// Check with regex.
			return CamelCaseRegex.IsMatch( name );
		}
	}
}
