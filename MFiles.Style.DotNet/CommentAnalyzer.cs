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
	public class CommentAnalyzer : DiagnosticAnalyzer
	{
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly RuleDescription CommentedSegments = new RuleDescription( "CommentedSegments", "Comments" );
        public static readonly RuleDescription NewlineBeforeComment = new RuleDescription( "NewlineBeforeComment", "Comments" );
        public static readonly RuleDescription SpacesBeforeTrailingComment = new RuleDescription( "SpacesBeforeTrailingComment", "Comments" );
        public static readonly RuleDescription CommentStartsWithSpace = new RuleDescription( "CommentStartsWithSpace", "Comments" );

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                        CommentedSegments.Rule,
                        NewlineBeforeComment.Rule,
                        SpacesBeforeTrailingComment.Rule,
                        CommentStartsWithSpace.Rule );
            }
        }

		public override void Initialize( AnalysisContext context )
		{
			context.RegisterSyntaxNodeAction( AnalyzeCodeBlocks, SyntaxKind.Block );
            context.RegisterSyntaxTreeAction( AnalyzeAllComments );
		}

        private static void AnalyzeAllComments( SyntaxTreeAnalysisContext context )
        {
            var root = context.Tree.GetRoot( context.CancellationToken );
            var comments = root.DescendantTrivia().Where( t => t.IsKind( SyntaxKind.SingleLineCommentTrivia ) );

            foreach( var comment in comments )
            {
                // If the comment doesn't start with double '//' we have no idea what it is.
                var str = comment.ToString();
                if( !str.StartsWith( "//" ) )
                    continue;

                // If the comment has space after slashes it's okay.
                if( str[ 2 ] == ' ' )
                    continue;

                // Create the diagnostic message and report it.
                var diagnostic = Diagnostic.Create(
                        CommentStartsWithSpace.Rule,
                        comment.GetLocation() );
                context.ReportDiagnostic( diagnostic );
            }
        }

		private static void AnalyzeCodeBlocks( SyntaxNodeAnalysisContext context )
		{
            int previousStatementEndLine = int.MinValue;
            int segmentStart = -1;
            int nodesInSegment = 0;
            SyntaxNode firstInSegment = null;
            SyntaxNode lastInSegment = null;

            foreach( var childNode in context.Node.ChildNodes() )
            {
                CheckLeadingCommentSpace( context, childNode );
                CheckTrailingCommentSpace( context, childNode );

                // Store the segment start if we're not tracking a segment currently.
                var lineSpan = childNode.GetLocation().GetLineSpan();
                if( segmentStart == -1 )
                {
                    segmentStart = lineSpan.StartLinePosition.Line;
                    previousStatementEndLine = lineSpan.EndLinePosition.Line;
                    firstInSegment = childNode;
                    lastInSegment = childNode;
                }

                // Check whether the current syntax node is attached to the previous node.
                if( lineSpan.StartLinePosition.Line <= previousStatementEndLine + 1 )
                {
                    // The nodes are attached. Find the next one.
                    previousStatementEndLine = lineSpan.EndLinePosition.Line;
                    lastInSegment = childNode;
                    nodesInSegment += 1;
                    continue;
                }

                // Segment ended so we know its full length. Make sure it has a comment.
                RequireComment( context, firstInSegment, lastInSegment );

                // Start a new segment.
                segmentStart = lineSpan.StartLinePosition.Line;
                previousStatementEndLine = lineSpan.EndLinePosition.Line;
                nodesInSegment = 1;
                firstInSegment = childNode;
                lastInSegment = childNode;
            }

            // No more statements so make sure the last segment has a comment as well.
            if( firstInSegment != null )
                RequireComment( context, firstInSegment, lastInSegment );
		}

        private static void CheckTrailingCommentSpace( SyntaxNodeAnalysisContext context, SyntaxNode childNode )
        {
            if( !childNode.HasTrailingTrivia )
                return;

            var trivia = childNode.GetTrailingTrivia().ToList();

            var badDistance = false;
            Location triviaLocation = null;

            // If the single line comment is attached directly it's bad.
            if( trivia.Count > 0 && trivia[ 0 ].IsKind( SyntaxKind.SingleLineCommentTrivia ) )
            {
                badDistance = true;
                triviaLocation = trivia[ 0 ].GetLocation();
            }

            // If the single line comment has whitespace in front that isn't 2 space, it's bad.
            if( trivia.Count > 1 &&
                trivia[ 0 ].IsKind( SyntaxKind.WhitespaceTrivia ) &&
                trivia[ 1 ].IsKind( SyntaxKind.SingleLineCommentTrivia ) &&
                trivia[ 0 ].ToString() != "  " )
            {
                badDistance = true;
                triviaLocation = trivia[ 1 ].GetLocation();
            }

            if( badDistance )
            {
                // Create the diagnostic message and report it.
                var diagnostic = Diagnostic.Create(
                        SpacesBeforeTrailingComment.Rule,
                        triviaLocation );
                context.ReportDiagnostic( diagnostic );
            }
        }

        private static void CheckLeadingCommentSpace( SyntaxNodeAnalysisContext context, SyntaxNode childNode )
        {
            if( !childNode.HasLeadingTrivia )
                return;

            var comments = childNode.GetLeadingTrivia()
                                .Where( trivia => trivia.IsKind( SyntaxKind.SingleLineCommentTrivia ) )
                                .ToList();

            SourceText sourceText = null;

            var previousLineNumber = int.MinValue;
            for( var i = 0; i < comments.Count; ++i )
            {
                var comment = comments[ i ];
                var currentLineNumber = comment.GetLocation().GetLineSpan().StartLinePosition.Line;

                // If this is continuation block, skip the checks.
                if( previousLineNumber == currentLineNumber - 1 )
                {
                    previousLineNumber = currentLineNumber;
                    continue;
                }

                // Store the previous line number as we won't need it during this iteration
                // anymore and we might continue out of it at some point.
                previousLineNumber = currentLineNumber;

                // If we haven't retrieved the source text yet, do so now.
                // The source should be the same for all the trivia here.
                if( sourceText == null )
                    sourceText = comment.GetLocation().SourceTree
                                        .GetText( context.CancellationToken );

                var lineAbove = sourceText.GetSubText( sourceText.Lines[ currentLineNumber - 1 ].Span )
                                        .ToString().Trim();

                // If the previous line is nothing but an opening brace, consider this okay.
                if( lineAbove == "{" || lineAbove == "" )
                    continue; 

                // Create the diagnostic message and report it.
                var diagnostic = Diagnostic.Create(
                        NewlineBeforeComment.Rule,
                        comment.GetLocation() );
                context.ReportDiagnostic( diagnostic );
            }
        }

        private static void RequireComment( SyntaxNodeAnalysisContext context, SyntaxNode firstInSegment, SyntaxNode lastInSegment )
        {
            // Try to find a leading comment for the segment.
            var hasComments = false;
            if( firstInSegment.HasLeadingTrivia )
            {
                // Get the list of all trivia. This includes comments, end of lines, whitespace, etc.
                var trivia = firstInSegment.GetLeadingTrivia().ToList();

                // Find the last comment. If one exists, calculate the amount of new lines afterwards.
                var newLines = int.MaxValue;
                var lastComment = trivia.FindLastIndex( item => item.IsKind( SyntaxKind.SingleLineCommentTrivia ) );
                if( lastComment >= 0 )
                    newLines = trivia.Skip( lastComment ).Count( item => item.IsKind( SyntaxKind.EndOfLineTrivia ) );

                // If new line was within 1 line break (no empty line between), consider it being the comment of the current segment.
                if( newLines <= 1 )
                    hasComments = true;
            }

            // If the segment has no comment, flag it for error.
            if( !hasComments )
            {
                // Create the diagnostic message and report it.
                var diagnostic = Diagnostic.Create(
                        CommentedSegments.Rule,
                        Location.Create(
                            context.Node.GetLocation().SourceTree,
                            TextSpan.FromBounds( firstInSegment.Span.Start, lastInSegment.Span.End ) ) );
                context.ReportDiagnostic( diagnostic );
            }
        }
    }
}
