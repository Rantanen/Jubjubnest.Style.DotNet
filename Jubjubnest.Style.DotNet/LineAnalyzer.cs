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
	public class LineAnalyzer : DiagnosticAnalyzer
	{
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly RuleDescription IndentWithTabs = new RuleDescription( nameof( IndentWithTabs ), "Indent" );
        public static readonly RuleDescription DoubleTabContinuationIndent = new RuleDescription( nameof( DoubleTabContinuationIndent ), "Indent" );

        public static readonly RuleDescription KeepLinesWithin120Characters = new RuleDescription( nameof( KeepLinesWithin120Characters ), "Newlines" );
        public static readonly RuleDescription BracesOnTheirOwnLine = new RuleDescription( nameof( BracesOnTheirOwnLine ), "Newlines" );

		public override ImmutableArray< DiagnosticDescriptor > SupportedDiagnostics =>
				ImmutableArray.Create(
					IndentWithTabs.Rule,
					DoubleTabContinuationIndent.Rule,
					KeepLinesWithin120Characters.Rule,
					BracesOnTheirOwnLine.Rule );

		public override void Initialize( AnalysisContext context )
		{
			context.RegisterSyntaxTreeAction( AnalyzeIndent );
			context.RegisterSyntaxNodeAction( AnalyzeBlocks, SyntaxKind.Block );
			context.RegisterSyntaxNodeAction( AnalyzePropertyBlock, SyntaxKind.PropertyDeclaration );
		}

		private static readonly Regex IndentRegex = new Regex( @"^[\t ]*" );

		private static void AnalyzePropertyBlock( SyntaxNodeAnalysisContext context )
		{
			var property = ( PropertyDeclarationSyntax )context.Node;

			var text = context.Node.SyntaxTree.GetText( context.CancellationToken );
			var parent = property;
			var openBrace = property.AccessorList.OpenBraceToken;
			var closeBrace = property.AccessorList.CloseBraceToken;
			CheckBraceLines( context, parent, openBrace, closeBrace, text );
		}

		private static void AnalyzeBlocks( SyntaxNodeAnalysisContext context )
		{
			var block = (BlockSyntax)context.Node;

			// Ensure braces are on their own lines.
			var text = context.Node.SyntaxTree.GetText( context.CancellationToken );
			var parent = block.Parent;
			var openBrace = block.OpenBraceToken;
			var closeBrace = block.CloseBraceToken;

			CheckBraceLines( context, parent, openBrace, closeBrace, text );

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
				var lastNewlineIndex = leadingTrivia.FindLastIndex( trivia => trivia.IsKind( SyntaxKind.EndOfLineTrivia ) );
				int lastLineIndex = lastNewlineIndex == -1 ? 0 : lastNewlineIndex + 1;

				// Once we know where the last line starts, we can find the whitespace at the start of that line.
				var whitespaceOfLastLine = leadingTrivia.FindIndex( lastLineIndex,
					trivia => trivia.IsKind( SyntaxKind.WhitespaceTrivia ) );

				var firstLineIndent = whitespaceOfLastLine == -1 ? "" : leadingTrivia[ whitespaceOfLastLine ].ToString();
				int firstIndent = GetTextLength( firstLineIndent );
				var secondLineIndent = IndentRegex.Match( secondLine ).Value;
				int expectedIndent = firstIndent + 8;

				if( GetTextLength( secondLineIndent ) < expectedIndent )
				{
					var start = statement.SpanStart + lines[ 0 ].Length + 1 + secondLineIndent.Length;
					var end = start + 1;
					var diagnostic = Diagnostic.Create(
						DoubleTabContinuationIndent.Rule,
						Location.Create( statement.SyntaxTree, TextSpan.FromBounds( start, end ) ) );
					context.ReportDiagnostic( diagnostic );
				}
			}
		}

		private static void CheckBraceLines( SyntaxNodeAnalysisContext context, SyntaxNode parent, SyntaxToken openBrace,
			SyntaxToken closeBrace, SourceText text )
		{
			var parentStartLine = parent.GetLocation().GetLineSpan().StartLinePosition.Line;
			var openLineNumber = openBrace.GetLocation().GetLineSpan().StartLinePosition.Line;
			var closeLineNumber = closeBrace.GetLocation().GetLineSpan().StartLinePosition.Line;

			if( parentStartLine != openLineNumber || openLineNumber != closeLineNumber )
			{
				var openLine = text.Lines[ openLineNumber ].ToString();
				var closeLine = text.Lines[ closeLineNumber ].ToString();

				if( openLine.Trim() != "{" )
				{
					var diagnostic = Diagnostic.Create(
						BracesOnTheirOwnLine.Rule,
						openBrace.GetLocation() );
					context.ReportDiagnostic( diagnostic );
				}

				if( closeLine.Trim() != "}" )
				{
					var diagnostic = Diagnostic.Create(
						BracesOnTheirOwnLine.Rule,
						closeBrace.GetLocation() );
					context.ReportDiagnostic( diagnostic );
				}
			}
		}

		private static readonly Regex Indent = new Regex( @"^\t*(?<space> +)" );

        private static void AnalyzeIndent( SyntaxTreeAnalysisContext context )
        {
	        var text = context.Tree.GetText( context.CancellationToken );

	        foreach( var line in text.Lines )
	        {
		        var lineText = line.ToString();
		        int treshold;
		        var length = GetTextLengthWith120Treshold( lineText, out treshold );
		        if( treshold != -1 )
		        {
			        var diagnostic = Diagnostic.Create(
				        KeepLinesWithin120Characters.Rule,
				        Location.Create( context.Tree,
					        TextSpan.FromBounds( line.Span.Start + treshold, line.Span.End ) ) );
					context.ReportDiagnostic( diagnostic );
		        }

		        var match = Indent.Match( line.ToString() );
		        if( match.Success )
		        {
					var start = match.Groups[ "space" ].Index;
					var end = start + match.Groups[ "space" ].Length;
					var diagnostic = Diagnostic.Create(
						IndentWithTabs.Rule,
						Location.Create( context.Tree,
							TextSpan.FromBounds( line.Start + start, line.Start + end ) ) );
					context.ReportDiagnostic( diagnostic );
		        }
	        }
        }

		/// <summary>
		/// Get the string display length.
		/// </summary>
		/// <param name="text">Text to resolve the length for.</param>
		/// <returns>String length assuming tab = 4 spaces.</returns>
		private static int GetTextLength( string text )
		{
			int charCount = 0;
			return GetTextLengthWith120Treshold( text, out charCount );
		}

		/// <summary>
		/// Get the string display length.
		/// </summary>
		/// <param name="text">Text to resolve the length for.</param>
		/// <returns>String length assuming tab = 4 spaces.</returns>
		private static int GetTextLengthWith120Treshold( string text, out int tresholdCharCount )
		{
			int length = 0;
			tresholdCharCount = 0;
			foreach( var c in text )
			{
				// Count this in the treshold if we're still below the length.
				if( length < 120 )
					tresholdCharCount++;

				// Figure out whether this is a multi-width display character.
				if( c == '\t' )
				{
					// Tab should end up at the next multiple of 4.
					// Add 4 to the length subtracted by the mod 4 to ensure the
					// final stays as a multiple of 4. If length is multiple of 4,
					// the remainder is 0 which causes the full + 4 on length.
					length += 4 - length % 4;
				}
				else
				{
					// Single-width character.
					length += 1;
				}
			}

			// If we ended up with less than 120 chars, reset the treshold to -1.
			if( length <= 120 )
				tresholdCharCount = -1;

			return length;
		}

    }
}
