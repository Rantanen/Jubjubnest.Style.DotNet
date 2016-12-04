using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jubjubnest.Style.DotNet.Test.Helpers
{
    public static class Code
    {
        public class CodeResult
        {
            public string Code { get; }

			public int PrefixLines { get; }

            private int PrefixLength { get; }
            private int CodeLength { get; }

            public CodeResult( string prefix, string code, string postfix )
            {
                prefix += "\r\n";
                postfix = "\r\n" + postfix;

                this.Code = prefix + code + postfix;
                this.PrefixLength = prefix.Length;
                this.CodeLength = code.Length;
	            this.PrefixLines = prefix.Count( c => c == '\n' ) + 1;
            }

            public TextSpan Span( int start, int end )
            {
                if( start < 0 )
                    start += PrefixLength;
                if( end < 0 )
                    end += PrefixLength;

                return TextSpan.FromBounds( PrefixLength + start, PrefixLength + end );
            }
        }

        public static CodeResult InClass( string code )
        {
            return new CodeResult(
@"		namespace Namespace
		{
			class Class
			{",
			code,
@"			}
		}" );
        }

        public static CodeResult InMethod( string code )
        {
            return new CodeResult(
@"	namespace Namespace
	{
		class Class
		{
			void Method()
			{",
				code,
@"			}
		}
	}" );
        }
    }
}
