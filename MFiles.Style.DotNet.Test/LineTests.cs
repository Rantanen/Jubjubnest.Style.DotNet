using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using MFiles.Style.DotNet;
using MFiles.Style.DotNet.Test.Helpers;

namespace MFiles.Style.DotNet.Test
{
	[TestClass]
	public class LineTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestLineWithSpaceIndent()
		{
            var code = Code.InMethod( "    foo();" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 1, LineAnalyzer.IndentWithTabs ) );
		}

		[TestMethod]
		public void TestLineWithMixedIndent()
		{
            var code = Code.InMethod( "\t    foo();" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 2, LineAnalyzer.IndentWithTabs ) );
		}

		[TestMethod]
		public void TestLineOver120WithSpaces()
		{
			var code = Code.InMethod( new string( ' ', 116 ) + "int foo;" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 1, LineAnalyzer.IndentWithTabs ),
					Warning( code, 0, 121, LineAnalyzer.KeepLinesWithin120Characters ) );
		}

		[TestMethod]
		public void TestLineOver30Tabs()
		{
			var code = Code.InMethod( new string( '\t', 29 ) + "int foo;" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 34, LineAnalyzer.KeepLinesWithin120Characters ) );
		}

		[TestMethod]
		public void TestContinuationLineWithSingleIndent()
		{
			var code = Code.InMethod( @"
				int foo =
					1;
			" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 6, LineAnalyzer.DoubleTabContinuationIndent ) );
		}

		[TestMethod]
		public void TestBracesNotAlone()
		{
			var code = Code.InMethod( @"
				if( foo ) {
				} else {
				}
			" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 15, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 5, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 12, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestTwoLinePropertyWithBracesOnSharedLines()
		{
			var code = Code.InClass( @"
				public string Foo {
					get; set; }
			" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 23, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 16, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestSingleLineAutomaticProperties()
		{
			var code = Code.InClass( @"
				public string Foo { get; set; }
			" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLinePropertyAccessors()
		{
			var code = Code.InClass( @"
				public string Foo
				{
					get { return foo; }
					set { foo = value; }
				}
			" );

			VerifyCSharpDiagnostic( code.Code );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new MFilesStyleDotNetCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
            return new LineAnalyzer();
		}
	}
}