using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Jubjubnest.Style.DotNet
{
	/// <summary>
	/// Rule description.
	/// </summary>
	public class RuleDescription
	{
		public RuleDescription( string rule, string category )
		{
#if DEBUG

			// Debug mode has everything enabled by default.
			this.Enabled = true;
#else

			// Release mode has everything disabled by default.
			this.Enabled = false;
#endif

			// Grab the localized resources.
			var title = new LocalizableResourceString(
					rule + "_Title", Resources.ResourceManager, typeof( Resources ) );
			var message = new LocalizableResourceString(
					rule + "_Message", Resources.ResourceManager, typeof( Resources ) );
			var description = new LocalizableResourceString(
					rule + "_Description", Resources.ResourceManager, typeof( Resources ) );

			// If description wasn't localized, set it to null for descriptor purposes.
			if( string.IsNullOrWhiteSpace( description.ToString() ) )
				description = null;

			// Store the string data.
			this.Id = "Jubjubnest_" + rule;
			this.Name = rule;
			this.Message = message.ToString();

			// Create the diagnostic descriptor for the actual rule.
			this.Rule = new DiagnosticDescriptor(
					Id,
					title, message, category,
					DiagnosticSeverity.Warning,
					isEnabledByDefault: this.Enabled,
					description: description );
		}

		/// <summary>
		/// Rule name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Rule message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Default enable/disable state.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Rule ID.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// The actual rule.
		/// </summary>
		public DiagnosticDescriptor Rule { get; }
	}
}
