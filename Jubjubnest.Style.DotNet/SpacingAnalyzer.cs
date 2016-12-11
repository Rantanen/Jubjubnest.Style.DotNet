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

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Analyser for checking spacing rules.
	/// </summary>
	[ DiagnosticAnalyzer( LanguageNames.CSharp ) ]
	public class SpacingAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>
		/// Spaces should be inserted inside brackets (round, curly, square, angle).
		/// </summary>
		public static RuleDescription SpacesWithinBrackets { get; } =
				new RuleDescription( "SpacesWithinBrackets", "Spacing" );

		/// <summary>
		/// Supported diagnostic rules.
		/// </summary>
		public override ImmutableArray< DiagnosticDescriptor > SupportedDiagnostics =>
				ImmutableArray.Create(
					SpacesWithinBrackets.Rule );

		/// <summary>
		/// Initialize the actions.
		/// </summary>
		/// <param name="context">Analysis context to register the actions with.</param>
		public override void Initialize( AnalysisContext context )
		{
			// Ignore for generated files.
			context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );

			// Register actions.
			context.RegisterSyntaxNodeAction( AnalyzeBrackets,
					SyntaxKind.AccessorList,
					SyntaxKind.ArgumentList,
					SyntaxKind.ArrayInitializerExpression,
					SyntaxKind.CollectionInitializerExpression,
					SyntaxKind.ComplexElementInitializerExpression,
					SyntaxKind.ObjectInitializerExpression,
					SyntaxKind.AttributeArgumentList,
					// SyntaxKind.AttributeList,
					SyntaxKind.BracketedArgumentList,
					SyntaxKind.BracketedParameterList,
					SyntaxKind.DoStatement,
					SyntaxKind.ForEachStatement,
					SyntaxKind.ForStatement,
					SyntaxKind.IfStatement,
					SyntaxKind.ParameterList,
					SyntaxKind.ParenthesizedExpression,
					// SyntaxKind.TypeArgumentList,
					SyntaxKind.WhileStatement
				);
		}


		/// <summary>
		/// Analyze brackets.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeBrackets( SyntaxNodeAnalysisContext context )
		{
			// Get the brackets.
			List< SyntaxToken > brackets = new List< SyntaxToken >();

			// Get the brackets depending on the node kind.
			switch( context.Node.Kind() )
			{
				case SyntaxKind.AccessorList:
					brackets.Add( ( (AccessorListSyntax) context.Node ).OpenBraceToken );
					brackets.Add( ( (AccessorListSyntax) context.Node ).CloseBraceToken );
					break;

				case SyntaxKind.ArgumentList:
					brackets.Add( ( (ArgumentListSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (ArgumentListSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.ArrayInitializerExpression:
				case SyntaxKind.CollectionInitializerExpression:
				case SyntaxKind.ComplexElementInitializerExpression:
				case SyntaxKind.ObjectInitializerExpression:
					brackets.Add( ( (InitializerExpressionSyntax) context.Node ).OpenBraceToken );
					brackets.Add( ( (InitializerExpressionSyntax) context.Node ).CloseBraceToken );
					break;

				case SyntaxKind.AttributeArgumentList:
					brackets.Add( ( (AttributeArgumentListSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (AttributeArgumentListSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.AttributeList:
					brackets.Add( ( (AttributeListSyntax) context.Node ).OpenBracketToken );
					brackets.Add( ( (AttributeListSyntax) context.Node ).CloseBracketToken );
					break;

				case SyntaxKind.BracketedArgumentList:
					brackets.Add( ( (BracketedArgumentListSyntax) context.Node ).OpenBracketToken );
					brackets.Add( ( (BracketedArgumentListSyntax) context.Node ).CloseBracketToken );
					break;

				case SyntaxKind.BracketedParameterList:
					brackets.Add( ( (BracketedParameterListSyntax) context.Node ).OpenBracketToken );
					brackets.Add( ( (BracketedParameterListSyntax) context.Node ).CloseBracketToken );
					break;

				case SyntaxKind.DoStatement:
					brackets.Add( ( (DoStatementSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (DoStatementSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.ForEachStatement:
					brackets.Add( ( (ForEachStatementSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (ForEachStatementSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.ForStatement:
					brackets.Add( ( (ForStatementSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (ForStatementSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.IfStatement:
					brackets.Add( ( (IfStatementSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (IfStatementSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.ParameterList:
					brackets.Add( ( (ParameterListSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (ParameterListSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.ParenthesizedExpression:
					brackets.Add( ( (ParenthesizedExpressionSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (ParenthesizedExpressionSyntax) context.Node ).CloseParenToken );
					break;

				case SyntaxKind.TypeArgumentList:
					brackets.Add( ( (TypeArgumentListSyntax) context.Node ).LessThanToken );
					brackets.Add( ( (TypeArgumentListSyntax) context.Node ).GreaterThanToken );
					break;

				case SyntaxKind.WhileStatement:
					brackets.Add( ( (WhileStatementSyntax) context.Node ).OpenParenToken );
					brackets.Add( ( (WhileStatementSyntax) context.Node ).CloseParenToken );
					break;
			}

			// Ignore empty brackets here.
			if( brackets[ 0 ].Span.End == brackets[ 1 ].Span.Start )
				return;

			// Check each bracket.
			foreach( var bracket in brackets )
				CheckBracket( context, bracket );
		}

		/// <summary>
		/// Check brackets for spacing rules.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="bracket">Bracket to check.</param>
		private static void CheckBracket(
			SyntaxNodeAnalysisContext context,
			SyntaxToken bracket )
		{

			// Figure out where the whitespace should be.
			int spanStart = -1;
			switch( bracket.Kind() )
			{
				// Open bracket.
				case SyntaxKind.OpenBraceToken:
				case SyntaxKind.OpenBracketToken:
				case SyntaxKind.OpenParenToken:
				case SyntaxKind.LessThanToken:
					spanStart = bracket.SpanStart + 1;
					break;

				// Close bracket.
				case SyntaxKind.CloseBraceToken:
				case SyntaxKind.CloseBracketToken:
				case SyntaxKind.CloseParenToken:
				case SyntaxKind.GreaterThanToken:
					spanStart = bracket.SpanStart - 1;
					break;

				default:
					throw new NotImplementedException();
			}

			// Stop analysis if there's whitespace found.
			var text = bracket.GetLocation()
							.SourceTree
							.GetText()
							.GetSubText( TextSpan.FromBounds( spanStart, spanStart + 1 ) )
							.ToString();
			if( string.IsNullOrEmpty( text.Trim() ) )
				return;

			// Get the bracket type name.
			string type;
			switch( bracket.Kind() )
			{
				case SyntaxKind.OpenBraceToken:
				case SyntaxKind.CloseBraceToken:
					type = "brace";
					break;

				case SyntaxKind.OpenBracketToken:
				case SyntaxKind.CloseBracketToken:
					type = "bracket";
					break;

				case SyntaxKind.OpenParenToken:
				case SyntaxKind.CloseParenToken:
					type = "parenthesis";
					break;

				case SyntaxKind.LessThanToken:
				case SyntaxKind.GreaterThanToken:
					type = "angle bracket";
					break;

				default:
					throw new NotImplementedException();
			}

			// Create the diagnostic message and report it.
			var diagnostic = Diagnostic.Create(
					SpacesWithinBrackets.Rule,
					Location.Create(
						bracket.GetLocation().SourceTree,
						TextSpan.FromBounds( spanStart, spanStart + 1 ) ),
					type );
			context.ReportDiagnostic( diagnostic );
		}
	}
}
