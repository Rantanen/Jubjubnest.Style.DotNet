using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Jubjubnest.Style.DotNet;
using Jubjubnest.Style.DotNet.Test.Helpers;

namespace Jubjubnest.Style.DotNet.Test
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
		public void TestLineWithTrailingWhitespace()
		{
			var code = "namespace Foo {}  ";

			VerifyCSharpDiagnostic( code, Warning( 1, 17, LineAnalyzer.NoTrailingWhitespace ) );

			VerifyCSharpFix( new TestEnvironment(), code, code.Trim() );
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
					1;" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 6, LineAnalyzer.DoubleTabContinuationIndent ) );
		}

		[TestMethod]
		public void TestMultipleUsingStatements()
		{
			var code = Code.InMethod( @"
				using( var x = Foo() )
				using( var y = Bar() )
				{
					Console.WriteLine( x, y );
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestBracesNotAlone()
		{
			var code = Code.InMethod( @"
				if( foo ) {
				} else {
				}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 15, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 5, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 12, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestCommentAfterClosingBrace()
		{
			var code = Code.InMethod( @"
				if( foo )
				{
				}  // end if" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithParenthesis()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				} );" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithExtraParameters()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				}, bar );" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithAdditionalInvocations()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				} ).ToList();" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithWhile()
		{
			var code = Code.InMethod( @"
				do
				{
					// Looping.
				} while( true );" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestTwoLinePropertyWithBracesOnSharedLines()
		{
			var code = Code.InClass( @"
				public string Foo {
					get; set; }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 23, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 16, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestSingleLineAutomaticProperties()
		{
			var code = Code.InClass( @"public string Foo { get; set; }" );

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

				public string Bar { get; set; }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLinePropertyAccessorsWithAttributes()
		{
			var code = Code.InClass( @"
				[Attribute]
				public string Foo
				{
					get { return foo; }
					set { foo = value; }
				}

				[Attribute]
				public string Bar { get; set; }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLineAutomaticPropertiesWithDefaultValue()
		{
			var code = Code.InClass( @"public string Foo { get; set; } = """";" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestParametersOnSameLine()
		{
			var code = Code.InClass( @"
				public string Foo( string a, string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 34, LineAnalyzer.ParametersOnTheirOwnLines, "b" ) );
		}

		[TestMethod]
		public void TestConstructorParametersOnSameLine()
		{
			var code = Code.InClass( @"
				public Test( string a, string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 28, LineAnalyzer.ParametersOnTheirOwnLines, "b" ) );
		}

		[TestMethod]
		public void TestParametersOnTheirOwnLine()
		{
			var code = Code.InClass( @"
				public Test(
					string a,
					string b
				)
				{
				}
				public string Foo(
					string a,
					string b
				)
				{
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestParameterParenSharingParameterLine()
		{
			var code = Code.InClass( @"
				public string Foo(
					string a,
					string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( 8, 15, LineAnalyzer.ClosingParameterParenthesesOnTheirOwnLines ) );
		}

		[TestMethod]
		public void TestConstructorParameterParenSharingParameterLine()
		{
			var code = Code.InClass( @"
				public Test(
					string a,
					string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( 8, 15, LineAnalyzer.ClosingParameterParenthesesOnTheirOwnLines ) );
		}

		[TestMethod]
		public void TestCodeWithUnixNewlines()
		{
			var code = Code.InMethod( "int a = 0;\nint b = 0;" );

			VerifyCSharpDiagnostic( code.Code, Warning( 7, 11, LineAnalyzer.UseWindowsLineEnding ) );
		}

		[TestMethod]
		public void TestCodeWithAnonymousBlocks()
		{
			var code = Code.InMethod( @"
				{
					int i = 0;
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestDisallowBaseConstructorOtherLineForMultiLineParameters()
		{
			var code = Code.InClass( @"
				public Test(
					int i
				)
					: base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 4, 6, LineAnalyzer.BaseConstructorCallToClosingLine, "Test" ) );
		}

		[TestMethod]
		public void TestAllowBaseConstructorSameLineForMultiLineParameters()
		{
			var code = Code.InClass( @"
				public Test(
					int i
				) : base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestDisallowBaseConstructorSameLineWhenOneParameter()
		{
			var code = Code.InClass( @"
				public Test( int i ) : base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 26, LineAnalyzer.BaseConstructorCallToNextLine, "Test" ) );
		}

		[TestMethod]
		public void TestAllowBaseConstructorOtherLineWhenOneParameter()
		{
			var code = Code.InClass( @"
				public Test( int i )
					: base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new LineCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
            return new LineAnalyzer();
		}
	}
}