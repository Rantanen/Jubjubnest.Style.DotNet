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
	public class SpacesWithinBracketsTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestMissingSpacesInAccessorList()
		{
            var code = Code.InClass( "public string Foo {get; set;}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 20, SpacingAnalyzer.SpacesWithinBrackets, "brace" ),
					Warning( code, 0, 28, SpacingAnalyzer.SpacesWithinBrackets, "brace" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInArgumentList()
		{
            var code = Code.InClass( "void Foo(int a, int b) {}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 10, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 21, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInArrayInitailizerExpression()
		{
            var code = Code.InMethod( "int[] arr = new [] {1, 2, 3}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 21, SpacingAnalyzer.SpacesWithinBrackets, "brace" ),
					Warning( code, 0, 27, SpacingAnalyzer.SpacesWithinBrackets, "brace" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInCollectionInitializer()
		{
            var code = Code.InMethod( "var l = new List< int > {1, 2, 3}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 26, SpacingAnalyzer.SpacesWithinBrackets, "brace" ),
					Warning( code, 0, 32, SpacingAnalyzer.SpacesWithinBrackets, "brace" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInComplexElementInitializer()
		{
            var code = Code.InMethod( "var l = new Dictionary< int, int > { {0, 1} }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 39, SpacingAnalyzer.SpacesWithinBrackets, "brace" ),
					Warning( code, 0, 42, SpacingAnalyzer.SpacesWithinBrackets, "brace" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInObjectInitializerExpression()
		{
            var code = Code.InMethod( "var f = new Foo {A = 1, B = 2}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 18, SpacingAnalyzer.SpacesWithinBrackets, "brace" ),
					Warning( code, 0, 29, SpacingAnalyzer.SpacesWithinBrackets, "brace" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInAttributeArgumentList()
		{
            var code = Code.InClass( "[ Foo(1, 2) ] public string Foo { get; set; }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 7, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 10, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInAttributeList()
		{
            var code = Code.InClass( "[Foo, Bar] public string Foo { get; set; }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 2, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ),
					Warning( code, 0, 9, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInBracketedArgumentList()
		{
            var code = Code.InMethod( "var i = foo[123]" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 13, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ),
					Warning( code, 0, 15, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInBracketedParameterList()
		{
            var code = Code.InClass( "public this[int a] { get { return a; } }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 13, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ),
					Warning( code, 0, 17, SpacingAnalyzer.SpacesWithinBrackets, "bracket" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInDoStatement()
		{
            var code = Code.InMethod( "do { } while(true);" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 14, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 17, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInForEachStatement()
		{
            var code = Code.InMethod( "foreach(int i in foo) {}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 9, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 20, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInForStatement()
		{
            var code = Code.InMethod( "for(int i = 0; i < 0; i++) {}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 5, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 25, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInIfStatement()
		{
            var code = Code.InMethod( "if(true) {}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 4, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 7, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInParameterList()
		{
            var code = Code.InMethod( "foo(1, 2, 3);" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 5, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 11, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInParenthesizedExpression()
		{
            var code = Code.InMethod( "int i = (1 + 2)" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 10, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 14, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInTypeArgumentList()
		{
            var code = Code.InMethod( "List<int> i = new List<int>();" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 6, SpacingAnalyzer.SpacesWithinBrackets, "angle bracket" ),
					Warning( code, 0, 8, SpacingAnalyzer.SpacesWithinBrackets, "angle bracket" ),
					Warning( code, 0, 24, SpacingAnalyzer.SpacesWithinBrackets, "angle bracket" ),
					Warning( code, 0, 26, SpacingAnalyzer.SpacesWithinBrackets, "angle bracket" ) );
		}

		[TestMethod]
		public void TestMissingSpacesInWhileStatement()
		{
            var code = Code.InMethod( "while(true) {}" );

            VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 7, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ),
					Warning( code, 0, 10, SpacingAnalyzer.SpacesWithinBrackets, "parenthesis" ) );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new MFilesStyleDotNetCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
            return new SpacingAnalyzer();
		}

        protected DiagnosticResult Warning( string type, int row, int col )
        {
			return new DiagnosticResult
			{
				Id = "MFiles_Style_DotNet_SpacesWithinBrackets",
				Message = String.Format( "Insert a space inside the {0}.", type ),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation( "Test0.cs", row, col ) }
			};
        }
	}
}