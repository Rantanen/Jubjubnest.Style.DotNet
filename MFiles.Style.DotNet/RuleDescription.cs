using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.Style.DotNet
{
    public class RuleDescription
    {
        public RuleDescription( string rule, string category )
        {
            this.Id = "Jubjubnest_" + rule;
            this.Name = rule;

            var title = new LocalizableResourceString(
                rule + "_Title", Resources.ResourceManager, typeof( Resources ) );
            var message = new LocalizableResourceString(
                rule + "_Message", Resources.ResourceManager, typeof( Resources ) );
            var description = new LocalizableResourceString(
                rule + "_Description", Resources.ResourceManager, typeof( Resources ) );

			if( string.IsNullOrWhiteSpace( description.ToString() ) )
				description = null;

            this.Message = message.ToString();

            this.Rule = new DiagnosticDescriptor(
                    Id,
                    title, message, category,
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: description );
        }

        public string Name { get; }

        public string Message { get; }

        public bool Enabled { get; set; } = true;

        public string Id { get; }

        public DiagnosticDescriptor Rule { get; }
    }
}
