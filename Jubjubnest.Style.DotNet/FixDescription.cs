using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Code fix description.
	/// </summary>
	public class FixDescription
	{
		/// <summary>
		/// Fix title.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// The rule this fix handles.
		/// </summary>
		public RuleDescription Rule { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rule">Diagnostic rule the fix is responsible for.</param>
		public FixDescription( RuleDescription rule )
		{
			// Store the rule.
			this.Rule = rule;

			// Load the resources.
			this.Title = new LocalizableResourceString(
					rule.Name + "_Fix", Resources.ResourceManager, typeof( Resources ) ).ToString();
		}

		/// <summary>
		/// Registers the fix for the diagnostic if the diagnostic matches the one this fix is responsible for.
		/// </summary>
		/// <param name="context">Code fix context.</param>
		/// <param name="fix">The fix method.</param>
		public void SolutionFix(
			CodeFixContext context,
			Func< CodeFixContext, Diagnostic, CancellationToken, Task< Solution > > fix )
		{
			// Process all diagnostics caused by the rule this fix is responsible for.
			foreach( var diagnostic in context.Diagnostics.Where( d => d.Id == this.Rule.Id ) )
			{
				// Register the code fix for the diagnostic.
				context.RegisterCodeFix(
						CodeAction.Create( this.Title, c => fix( context, diagnostic, c ), this.Rule.Id ),
						diagnostic );
			}
		}

		/// <summary>
		/// Registers the fix for the diagnostic if the diagnostic matches the one this fix is responsible for.
		/// </summary>
		/// <param name="context">Code fix context.</param>
		/// <param name="fix">The fix method.</param>
		public void DocumentFix(
			CodeFixContext context,
			Func< CodeFixContext, Diagnostic, CancellationToken, Task< Document > > fix )
		{
			// Process all diagnostics caused by the rule this fix is responsible for.
			foreach( var diagnostic in context.Diagnostics.Where( d => d.Id == this.Rule.Id ) )
			{
				// Register the code fix for the diagnostic.
				context.RegisterCodeFix(
						CodeAction.Create( this.Title, c => fix( context, diagnostic, c ), this.Rule.Id ),
						diagnostic );
			}
		}
	}
}
