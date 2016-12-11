using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Fixes line analyzer issues.
	/// </summary>
	[ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof( LineCodeFixProvider ) ), Shared]
	public class LineCodeFixProvider : CodeFixProvider
	{
		/// <summary>
		/// Fixes trailing whitespaces.
		/// </summary>
		public static FixDescription FixTrailingWhitespace { get; } =
				new FixDescription( LineAnalyzer.NoTrailingWhitespace );

		/// <summary>
		/// Diagnostics fixable by this fix provider.
		/// </summary>
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
				ImmutableArray.Create( LineAnalyzer.NoTrailingWhitespace.Id );

		/// <summary>
		/// Returns a provider used for automatically fixing all issues.
		/// </summary>
		/// <returns>Returns the fix all provider.</returns>
		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		/// <summary>
		/// Registers the code fixes.
		/// </summary>
		/// <param name="context">Code fix context.</param>
		/// <returns>Asynchronous task.</returns>
		public sealed override Task RegisterCodeFixesAsync( CodeFixContext context )
		{
			// Register the rules as required.
			FixTrailingWhitespace.DocumentFix( context, this.RemoveTrailingWhitespace );

			// Return an empty task 'cos we have no idea how to do this properly.
			// This method is fully synchronous due to VS APIs.
			return Task.Run( () => { } );
		}

		/// <summary>
		/// Removes trailing whitespace.
		/// </summary>
		/// <param name="context">Fix context.</param>
		/// <param name="diagnostic">Diagnostic to handle.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Fixed document.</returns>
		private async Task< Document > RemoveTrailingWhitespace(
			CodeFixContext context,
			Diagnostic diagnostic,
			CancellationToken cancellationToken )
		{
			// Find the type declaration identified by the diagnostic.
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var root = await context.Document.GetSyntaxRootAsync( cancellationToken ).ConfigureAwait( false );
			var node = root.FindNode( diagnosticSpan, true, true );
			var oldTrivia = node.GetTrailingTrivia();

			// Skip the whitespace trivia at the end.
			var newTrivia = oldTrivia
					.Reverse()
					.SkipWhile( t => t.IsKind( SyntaxKind.WhitespaceTrivia ) )
					.Reverse();

			// Generate the replacement.
			var newNode = node.WithTrailingTrivia( newTrivia );

			// Get the document tree we'll replace.
			var newRoot = root.ReplaceNode( node, newNode );

			return context.Document.WithSyntaxRoot( newRoot );
		}
	}
}