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
	/// Analyzes the lines textually.
	/// </summary>
	[ DiagnosticAnalyzer( LanguageNames.CSharp ) ]
	public class LineAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>Indent with tabs instead of spaces.</summary>
		public static RuleDescription IndentWithTabs { get; } =
				new RuleDescription( nameof( IndentWithTabs ), "Indent" );

		/// <summary>Continuation lines should be indented with double tab.</summary>
		public static RuleDescription DoubleTabContinuationIndent { get; } =
				new RuleDescription( nameof( DoubleTabContinuationIndent ), "Indent" );

		/// <summary>There should be no trailing spaces.</summary>
		public static RuleDescription NoTrailingWhitespace { get; } =
				new RuleDescription( nameof( NoTrailingWhitespace ), "Spacing" );

		/// <summary>Keep lines within 120 characters.</summary>
		public static RuleDescription KeepLinesWithin120Characters { get; } =
				new RuleDescription( nameof( KeepLinesWithin120Characters ), "Newlines" );

		/// <summary>Opening and closing braces should be on their own lines.</summary>
		public static RuleDescription BracesOnTheirOwnLine { get; } =
				new RuleDescription( nameof( BracesOnTheirOwnLine ), "Newlines" );

		/// <summary>Parameters should be on their own lines.</summary>
		public static RuleDescription ParametersOnTheirOwnLines { get; } =
				new RuleDescription( nameof( ParametersOnTheirOwnLines ), "Newlines" );

		/// <summary>
		/// Supported diagnostic rules.
		/// </summary>
		public override ImmutableArray< DiagnosticDescriptor > SupportedDiagnostics =>
				ImmutableArray.Create(
					IndentWithTabs.Rule,
					DoubleTabContinuationIndent.Rule,
					NoTrailingWhitespace.Rule,
					KeepLinesWithin120Characters.Rule,
					BracesOnTheirOwnLine.Rule,
					ParametersOnTheirOwnLines.Rule );

		/// <summary>
		/// Initialize the analyzer.
		/// </summary>
		/// <param name="context">Analysis context the analysis actions are registered on.</param>
		public override void Initialize( AnalysisContext context )
		{
			// Register the actions.
			context.RegisterSyntaxTreeAction( AnalyzeLines );
			context.RegisterSyntaxNodeAction( AnalyzeBlocks, SyntaxKind.Block );
			context.RegisterSyntaxNodeAction( AnalyzePropertyBlock, SyntaxKind.PropertyDeclaration );
			context.RegisterSyntaxNodeAction( AnalyzeMethodParameters, SyntaxKind.MethodDeclaration );
		}

		/// <summary>
		/// Analyze method parameters.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeMethodParameters( SyntaxNodeAnalysisContext context )
		{
			// Grab the property syntax.
			var method = ( MethodDeclarationSyntax )context.Node;

			// Examine all parameters and the liens they are on.
			int previousParamLine = int.MinValue;
			foreach( var parameter in method.ParameterList.Parameters )
			{
				// Resolve the parameter line number.
				var currentLine = parameter.GetLocation().GetLineSpan().StartLinePosition.Line;

				// Check there was a previou parameter.
				if( previousParamLine != int.MinValue )
				{
					// Previous param exists.
					if( currentLine == previousParamLine )
					{
						// Parameter shares line with the previous one. Report diagnostic.
						var diagnostic = Diagnostic.Create(
								ParametersOnTheirOwnLines.Rule,
								parameter.GetLocation(),
								parameter.Identifier.ToString() );
						context.ReportDiagnostic( diagnostic );
					}
				}

				// Store the parameter line number for the next iteration.
				previousParamLine = currentLine;
			}
		}

		/// <summary>
		/// Analyze property blocks to ensure the braces are on their own lines.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzePropertyBlock( SyntaxNodeAnalysisContext context )
		{
			// Grab the property syntax.
			var property = ( PropertyDeclarationSyntax )context.Node;

			// If the accessor list is null, skip the whole check.
			if( property.AccessorList == null )
				return;

			// Ensure braces are on their own lines.
			CheckBraceLines(
					context,
					property,
					property.AccessorList.OpenBraceToken,
					property.AccessorList.CloseBraceToken );
		}

		/// <summary>
		/// Regex for gathering all the indent in front of the statement.
		/// </summary>
		private static readonly Regex indentRegex = new Regex( @"^[\t ]*" );

		/// <summary>
		/// Analyze the statements in the block for continuation lines.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeBlocks( SyntaxNodeAnalysisContext context )
		{
			// Grab the block syntax node.
			var block = (BlockSyntax)context.Node;

			// Ensure braces are on their own lines.
			CheckBraceLines(
					context,
					block.Parent,
					block.OpenBraceToken,
					block.CloseBraceToken );

			// Check each statement for continuation lines.
			foreach( var statement in block.Statements )
			{
				// Skip the flow control statements. These are expected to be multi-line and
				// don't require double-indent.
				if( statement.IsKind( SyntaxKind.IfStatement ) ||
					statement.IsKind( SyntaxKind.ForEachStatement ) ||
					statement.IsKind( SyntaxKind.ForStatement ) ||
					statement.IsKind( SyntaxKind.DoStatement ) ||
					statement.IsKind( SyntaxKind.SwitchStatement ) ||
					statement.IsKind( SyntaxKind.WhileStatement ) )
				{
					// Flow control statement. Skip.
					continue;
				}

				// There should be leading whitespace trivia.
				if( !statement.HasLeadingTrivia )
					continue;

				// If this is a single-line statement, skip it. There will be no continuation line.
				var lines = statement.ToString().Split( '\n' );
				if( lines.Length == 1 )
					continue;

				// If the second line is opening brace, it should signal to the user that this is a
				// continuation statement and we can skip the indent rules.
				var secondLine = lines[ 1 ];
				if( secondLine.Trim() == "{" || string.IsNullOrWhiteSpace( secondLine ) )
					continue;

				// Get the whitespace preceding the statement.
				// There might be a lot of trivia preceding the statement. We're currently insterested only of
				// the whitespace on the last line.

				// First figure out where the last line begins from.
				var leadingTrivia = statement.GetLeadingTrivia().ToList();
				var lastNewlineIndex = leadingTrivia.FindLastIndex(
						trivia => trivia.IsKind( SyntaxKind.EndOfLineTrivia ) );
				int lastLineIndex = lastNewlineIndex == -1 ? 0 : lastNewlineIndex + 1;

				// Once we know where the last line starts, we can find the whitespace at the start of that line.
				var whitespaceOfLastLine = leadingTrivia.FindIndex(
						lastLineIndex,
						trivia => trivia.IsKind( SyntaxKind.WhitespaceTrivia ) );

				// Calculate the expected indent for the second line.
				var firstLineIndent = whitespaceOfLastLine == -1
						? "" : leadingTrivia[ whitespaceOfLastLine ].ToString();
				int firstIndent = SyntaxHelper.GetTextLength( firstLineIndent );
				var secondLineIndent = indentRegex.Match( secondLine ).Value;
				int expectedIndent = firstIndent + 8;

				// Check whether the second line fulfills the indent requirement.
				if( SyntaxHelper.GetTextLength( secondLineIndent ) < expectedIndent )
				{
					// Indent requirement not fulfilled. Report an issue.
					var start = statement.SpanStart + lines[ 0 ].Length + 1 + secondLineIndent.Length;
					var end = start + 1;
					var diagnostic = Diagnostic.Create(
							DoubleTabContinuationIndent.Rule,
							Location.Create( statement.SyntaxTree, TextSpan.FromBounds( start, end ) ) );
					context.ReportDiagnostic( diagnostic );
				}
			}
		}

		/// <summary>
		/// Close brace validation regex.
		/// </summary>
		private static readonly Regex validCloseBrace = new Regex( @"^\s*\}[\s);]*(?://.*)?$" );

		/// <summary>
		/// Check whether the braces are on their own lines.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		/// <param name="parent">Parent node.</param>
		/// <param name="openBrace">Open brace.</param>
		/// <param name="closeBrace">Close brace.</param>
		private static void CheckBraceLines(
			SyntaxNodeAnalysisContext context,
			SyntaxNode parent,
			SyntaxToken openBrace,
			SyntaxToken closeBrace )
		{
			// Get the line numbers for the variou braces.
			var parentStartLine = parent.GetLocation().GetLineSpan().StartLinePosition.Line;
			var openLineNumber = openBrace.GetLocation().GetLineSpan().StartLinePosition.Line;
			var closeLineNumber = closeBrace.GetLocation().GetLineSpan().StartLinePosition.Line;

			// Check whether the braces are on single line.
			// We allow single line bracing in properties for example.
			if( parentStartLine == openLineNumber && openLineNumber == closeLineNumber )
				return;

			// Get the text for the syntax tree.
			var text = context.Node.SyntaxTree.GetText( context.CancellationToken );

			// Check the open line.
			var openLine = text.Lines[ openLineNumber ].ToString();
			if( openLine.Trim() != "{" )
			{
				// Open brace is not alone on its line. Report an error.
				var diagnostic = Diagnostic.Create(
						BracesOnTheirOwnLine.Rule,
						openBrace.GetLocation() );
				context.ReportDiagnostic( diagnostic );
			}

			// Check the close brace line.
			var closeLine = text.Lines[ closeLineNumber ].ToString();
			if( ! validCloseBrace.IsMatch( closeLine ) )
			{
				// Close brace isn't alone on its line. Report an error.
				var diagnostic = Diagnostic.Create(
						BracesOnTheirOwnLine.Rule,
						closeBrace.GetLocation() );
				context.ReportDiagnostic( diagnostic );
			}
		}

		/// <summary>
		/// Regex for capturing the space indenting.
		/// </summary>
		private static readonly Regex indent = new Regex( @"^\t*(?<space> +)" );

		/// <summary>
		/// Regex for checking for trailing whitespace.
		/// </summary>
		private static readonly Regex hasTrailingWhitespace = new Regex( @"\s+$" );

		/// <summary>
		/// Analyze each line textually.
		/// </summary>
		/// <param name="context">Analysis context.</param>
		private static void AnalyzeLines( SyntaxTreeAnalysisContext context )
		{
			// Get the text for the file.
			var text = context.Tree.GetText( context.CancellationToken );

			// Check each line.
			foreach( var line in text.Lines )
			{
				// Chech whether the line stays withint he 120 character limit.
				var lineText = line.ToString();
				int treshold;
				SyntaxHelper.GetTextLengthWith120Treshold( lineText, out treshold );
				if( treshold != -1 )
				{
					// Line exceeds 120 characters. Report the error.
					var diagnostic = Diagnostic.Create(
							KeepLinesWithin120Characters.Rule,
							Location.Create( context.Tree,
								TextSpan.FromBounds( line.Span.Start + treshold, line.Span.End ) ) );
					context.ReportDiagnostic( diagnostic );
				}

				// Check whether there are space indenting.
				var match = indent.Match( lineText );
				if( match.Success )
				{
					// Space indenting. REport error.
					var start = match.Groups[ "space" ].Index;
					var end = start + match.Groups[ "space" ].Length;
					var diagnostic = Diagnostic.Create(
							IndentWithTabs.Rule,
							Location.Create( context.Tree,
								TextSpan.FromBounds( line.Start + start, line.Start + end ) ) );
					context.ReportDiagnostic( diagnostic );
				}

				// Check for trailing whitespace.
				var trailingMatch = hasTrailingWhitespace.Match( lineText );
				if( trailingMatch.Success )
				{
					// Trailing whitespace. Report error.
					var diagnostic = Diagnostic.Create(
							NoTrailingWhitespace.Rule,
							Location.Create( context.Tree,
								TextSpan.FromBounds(
									line.Start + lineText.Length - trailingMatch.Length,
									line.End ) ) );
					context.ReportDiagnostic( diagnostic );
				}
			}
		}

	}
}
