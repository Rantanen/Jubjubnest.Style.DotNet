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
	[DiagnosticAnalyzer( LanguageNames.CSharp )]
	public class NamingAnalyzer : DiagnosticAnalyzer
	{
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly RuleDescription NameTypesWithPascalCasing = new RuleDescription( nameof( NameTypesWithPascalCasing ), "Naming" );
        public static readonly RuleDescription NameMethodsWithPascalCasing = new RuleDescription( nameof( NameMethodsWithPascalCasing ), "Naming" );
        public static readonly RuleDescription NameNamespacesWithPascalCasing = new RuleDescription( nameof( NameNamespacesWithPascalCasing ), "Naming" );
        public static readonly RuleDescription NameVariablesWithCamelCase = new RuleDescription( nameof( NameVariablesWithCamelCase ), "Naming" );
        public static readonly RuleDescription NamePropertiesWithPascalCase = new RuleDescription( nameof( NamePropertiesWithPascalCase ), "Naming" );
        public static readonly RuleDescription NameEventsWithPascalCase = new RuleDescription( nameof( NameEventsWithPascalCase ), "Naming" );
        public static readonly RuleDescription NameFieldsWithCamelCase = new RuleDescription( nameof( NameFieldsWithCamelCase ), "Naming" );
        public static readonly RuleDescription NameConstantsWithCapitalCase = new RuleDescription( nameof( NameConstantsWithCapitalCase ), "Naming" );
        public static readonly RuleDescription NameEnumValuesWithPascalCase = new RuleDescription( nameof( NameEnumValuesWithPascalCase ), "Naming" );
        public static readonly RuleDescription NameExceptionsWithExceptionSuffix = new RuleDescription( nameof( NameExceptionsWithExceptionSuffix ), "Naming" );
        public static readonly RuleDescription NameInterfacesWithIPrefix = new RuleDescription( nameof( NameInterfacesWithIPrefix ), "Naming" );
        public static readonly RuleDescription NameTypeParameterWithTPrefix = new RuleDescription( nameof( NameTypeParameterWithTPrefix ), "Naming" );
        public static readonly RuleDescription NameTypeParameterWithDescriptiveName = new RuleDescription( nameof( NameTypeParameterWithDescriptiveName ), "Naming" );

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

		public override void Initialize( AnalysisContext context )
		{
			context.RegisterSymbolAction( AnalyzeTypeName, SymbolKind.NamedType );
			context.RegisterSymbolAction( AnalyzeFieldName, SymbolKind.Field );
			context.RegisterSymbolAction( AnalyzePropertyName, SymbolKind.Property );
			context.RegisterSymbolAction( AnalyzeMethodName, SymbolKind.Method );
			context.RegisterSymbolAction( AnalyzeNamespaceName, SymbolKind.Namespace );
			context.RegisterSymbolAction( AnalyzeEventName, SymbolKind.Event );

			context.RegisterSyntaxNodeAction( AnalyzeEnumValues, SyntaxKind.EnumMemberDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeVariableNames, SyntaxKind.VariableDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeParameterNames, SyntaxKind.MethodDeclaration );
		}

		private static void AnalyzeEnumValues( SyntaxNodeAnalysisContext context )
		{
			var enumSyntax = ( EnumMemberDeclarationSyntax )context.Node;
			var enumName = enumSyntax.Identifier.ToString();

			if( !IsPascalCase( enumName ) )
			{
				var diagnostic = Diagnostic.Create(
						NameEnumValuesWithPascalCase.Rule,
						enumSyntax.Identifier.GetLocation(),
						enumName );
				context.ReportDiagnostic( diagnostic );
			}
		}

		private static void AnalyzeParameterNames( SyntaxNodeAnalysisContext context )
		{
			var method = (MethodDeclarationSyntax)context.Node;
			foreach( var parameter in method.ParameterList.Parameters )
				CheckName( context, parameter.Identifier, NameVariablesWithCamelCase, IsCamelCase );

			if( method.TypeParameterList == null )
				return;

			foreach( var typeParameter in method.TypeParameterList.Parameters )
			{
				CheckName( context, typeParameter.Identifier, NameTypesWithPascalCasing, IsPascalCase, "type parameter" );
				CheckPrefix( "T", typeParameter.Identifier, NameTypeParameterWithTPrefix, IsPascalCase, context.ReportDiagnostic );
			}
		}

		private static void AnalyzeVariableNames( SyntaxNodeAnalysisContext context )
		{
			var variableSyntax = (VariableDeclarationSyntax)context.Node;
			foreach( var variable in variableSyntax.Variables )
				CheckName( context, variable.Identifier, NameVariablesWithCamelCase, IsCamelCase );
		}

		private static void AnalyzeTypeName( SymbolAnalysisContext context )
        {
	        var syntaxRefs =
					context.Symbol.DeclaringSyntaxReferences
						.Select( r => r.GetSyntax( context.CancellationToken ) )
						.ToList();

	        CheckName( context, NameTypesWithPascalCasing, IsPascalCase, SyntaxHelper.GetItemType( syntaxRefs.First() ) );

	        foreach( var syntax in syntaxRefs )
	        {
		        if( syntax.IsKind( SyntaxKind.ClassDeclaration ) )
		        {
			        var classSyntax = (ClassDeclarationSyntax)syntax;
			        CheckExceptionName( context, classSyntax );
		        }

		        if( syntax.IsKind( SyntaxKind.InterfaceDeclaration ) )
		        {
					var interfaceSyntax = ( InterfaceDeclarationSyntax )syntax;
			        CheckPrefix( "I", interfaceSyntax.Identifier, NameInterfacesWithIPrefix, IsPascalCase, context.ReportDiagnostic );
		        }
	        }
        }

		private static void CheckExceptionName( SymbolAnalysisContext context, ClassDeclarationSyntax classSyntax )
		{
			if( classSyntax.BaseList == null ||
				!classSyntax.BaseList.Types.Any( bt => bt.ToString().EndsWith( "Exception" ) ) )
			{
				return;
			}

			if( context.Symbol.Name.EndsWith( "Exception" ) )
				return;

			foreach( var l in context.Symbol.Locations )
			{
				var diagnostic = Diagnostic.Create(
					NameExceptionsWithExceptionSuffix.Rule,
					l, context.Symbol.Name );
				context.ReportDiagnostic( diagnostic );
			}
		}

		private static void CheckPrefix( string prefix, SyntaxToken token, RuleDescription rule, Func< string, bool > predicate, Action< Diagnostic > report )
		{
			var name = token.ToString();

			if( name.StartsWith( prefix ) &&
				name.Length > prefix.Length &&
				predicate( name.Substring( prefix.Length, 1 ) ) )
			{
				return;
			}

			var diagnostic = Diagnostic.Create(
				rule.Rule,
				token.GetLocation(), name );
			report( diagnostic );
		}

		private static void AnalyzeFieldName( SymbolAnalysisContext context )
        {
	        CheckName( context, NameFieldsWithCamelCase, IsCamelCase );
        }

        private static void AnalyzePropertyName( SymbolAnalysisContext context )
        {
	        CheckName( context, NamePropertiesWithPascalCase, IsPascalCase );
        }

        private static void AnalyzeMethodName( SymbolAnalysisContext context )
        {
	        CheckName( context, NameMethodsWithPascalCasing, IsPascalCase );
        }

        private static void AnalyzeNamespaceName( SymbolAnalysisContext context )
        {
	        CheckName( context, NameNamespacesWithPascalCasing, IsPascalCase );
        }

        private static void AnalyzeEventName( SymbolAnalysisContext context )
        {
	        CheckName( context, NameEventsWithPascalCase, IsPascalCase );
        }

        private static void AnalyzeTypeParameter( SymbolAnalysisContext context )
        {
        }

		private static void CheckName( SymbolAnalysisContext context, RuleDescription ruleDescription,
			Func< string, bool > predicate, params object[] args )
		{
			// If this is implicit symbol, skip it for naming checks.
			if( context.Symbol.IsImplicitlyDeclared )
				return;

			if( ! context.Symbol.CanBeReferencedByName )
				return;

	        if( !predicate( context.Symbol.Name ) )
	        {
		        foreach( var location in context.Symbol.Locations )
		        {
			        var formatParams = new List< object >( args ) { context.Symbol.Name };
			        var diagnostic = Diagnostic.Create(
							ruleDescription.Rule,
							location,
							formatParams.ToArray() );
					context.ReportDiagnostic( diagnostic );
		        }
	        }
		}

		private static void CheckName( SyntaxNodeAnalysisContext context, SyntaxToken identifier, RuleDescription ruleDescription, Func< string, bool > predicate, params object[] args )
		{
			var name = identifier.ToString();
	        if( !predicate( name ) )
	        {
				var formatParams = new List< object >( args ) { name };
				var diagnostic = Diagnostic.Create(
						ruleDescription.Rule,
						identifier.GetLocation(),
						formatParams.ToArray() );
				context.ReportDiagnostic( diagnostic );
	        }
		}

		private static readonly Regex pascalCase = new Regex( @"
			^(?>
				[A-Z]              # Normal pascal casing
				[a-z]+
			|
				[A-Z][A-Z]?        # Abbreviation
				(?>                # Abbreviation must be followed by normal pascal casing or end of name.
					[A-Z][a-z]+
				|
					$
				)
			)+$
		", RegexOptions.IgnorePatternWhitespace );

		private static bool IsPascalCase( string name )
		{
			return pascalCase.IsMatch( name );
		}

		private static readonly Regex camelCase = new Regex( @"^[a-z]+(?>[A-Z][A-Z]?[a-z]+)*[A-Z]?$" );

		private static bool IsCamelCase( string name )
		{
			return camelCase.IsMatch( name );
		}

		private static readonly Regex capitalCase = new Regex( @"^[A-Z]+(?>_[A-Z]+)*$" );

		private static bool IsCapitalCase( string name )
		{
			return capitalCase.IsMatch( name );
		}
    }
}
