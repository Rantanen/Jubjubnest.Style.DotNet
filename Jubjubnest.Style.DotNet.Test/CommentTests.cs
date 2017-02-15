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
	public class CommentTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestMissingComments()
		{
            var code = Code.InMethod( @"
                int i = 0;
                int u = 0;
            " );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestMissingCommentsTwoBlocks()
		{
            var code = Code.InMethod( @"
                int i = 0;
                int u = 0;

                int a = 0;
                int b = 0;
            " );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentedSegments ), Warning( code, 4, 17, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestCommentTooFar()
		{
            var code = Code.InMethod( @"
                // Foo

                int a = 1;
                int b = 2;
            " );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 3, 17, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestExistingComments()
		{
            var code = Code.InMethod( @"
                // Foo
                int a = 1;
                int b = 2;
            " );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestExistingCommentsTwoBlocks()
		{
            var code = Code.InMethod( @"
                // Foo
                int i = 0;
                int u = 0;

                // Bar
                int a = 0;
                int b = 0;
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentsWithinIfBlock()
		{
            var code = Code.InMethod( @"
                if( true )
                {
                    int i = 0;
                    int u = 0;
                }
            " );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentedSegments ), Warning( code, 3, 21, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestExistingCommentsWithinIfBlock()
		{
            var code = Code.InMethod( @"
                // if statement
                if( true )
                {
                    // body
                    int i = 0;
                    int u = 0;
                }
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentsForIfStatement()
		{
            var code = Code.InMethod( @"
                if( true )
                    Foo();
            " );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestExistingCommentsForIfStatement()
		{
            var code = Code.InMethod( @"
                // if
                if( true )
                    Foo();
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentForFinalReturn()
		{
            var code = Code.InMethod( @"
                // variables
				int x = 0;
				int y = 0;

				return x;
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentForFinalThrow()
		{
            var code = Code.InMethod( @"
                // variables
				int x = 0;
				int y = 0;

				throw new Exception( null );
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentForFinalMultiLineThrow()
		{
            var code = Code.InMethod( @"
                // variables
				int x = 0;
				int y = 0;

				throw new Exception(
						null,
						null );
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentForSingleStatementMethod()
		{
			var code = Code.InMethod( @"x = 1;" );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentForSingleLineBlock()
		{
			var code = Code.InClass( @"
				public int Y
				{
					get { return y; }
					set { y = value; }
				}
			" );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestMissingCommentsInLambdaBody()
		{
            var code = Code.InMethod( @"
                Action< string > action = ( str ) =>
                {
                    Console.WriteLine( str );
                    Console.ReadKey();
                };
            " );

            VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentedSegments ), Warning( code, 3, 21, CommentAnalyzer.CommentedSegments ) );
		}

		[TestMethod]
		public void TestExistingCommentsInLambdaBody()
		{
            var code = Code.InMethod( @"
                // Lambda
                Action< string > action = ( str ) =>
                {
                    // Body
                    Console.WriteLine( str );
                    Console.ReadKey();
                };
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCommentsWithoutNewline()
		{
            var code = Code.InMethod( @"
                // Foo
                int a = 0;
                // Bar
                int b = 1;
            " );

            VerifyCSharpDiagnostic( code.Code, Warning( code, 3, 17, CommentAnalyzer.NewlineBeforeComment ) );
		}

		[TestMethod]
		public void TestCommentsPrecededByBrace()
		{
            var code = Code.InMethod( @"
                // Scope
                {
                    // Foo
                    int a = 0;
                    int b = 1;
                }
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCommentsPrecededByBracePlusStuff()
		{
            var code = Code.InMethod( @"
                // Scope
                {  // start stuff.
                    // Foo
                    int a = 0;
                    int b = 1;
                }
            " );

            VerifyCSharpDiagnostic( code.Code, Warning( code, 3, 21, CommentAnalyzer.NewlineBeforeComment ) );
		}

		[TestMethod]
		public void TestTrailingCommentTooClose()
		{
            var code = Code.InMethod( @"
                // Block
                int a = 0;// Foo
                int a = 0; // Foo
            " );

            VerifyCSharpDiagnostic( code.Code,
                    Warning( code, 2, 27, CommentAnalyzer.SpacesBeforeTrailingComment ),
                    Warning( code, 3, 28, CommentAnalyzer.SpacesBeforeTrailingComment ) );
		}

		[TestMethod]
		public void TestTrailingCommentTooFar()
		{
            var code = Code.InMethod( @"
                // Block
                int a = 0;   // Foo
                int a = 0;    // Foo
            " );

            VerifyCSharpDiagnostic( code.Code,
                    Warning( code, 2, 30, CommentAnalyzer.SpacesBeforeTrailingComment ),
                    Warning( code, 3, 31, CommentAnalyzer.SpacesBeforeTrailingComment ) );
		}

		[TestMethod]
		public void TestTrailingComments()
		{
            var code = Code.InMethod( @"
                // Foo
                int a = 0;  // a
                int b = 1;  // b

                // Bar
                int a = 0;  // a
                int b = 1;  // b
            " );

            VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCommentWithoutSpace()
		{
            var code = Code.InMethod( @"
                //Foo
                int foo = 0;
            " );

            VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 17, CommentAnalyzer.CommentStartsWithSpace ) );
		}

		[TestMethod]
		public void TestMissingCommentOnDelegationCall()
		{
            var code = Code.InClass( @"
				public void Foo()
				{
					AnotherFoo();
				}

				public void AnotherFoo()
				{
					this.Bar();
				}
            " );

			VerifyCSharpDiagnostic( code.Code );
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new CommentAnalyzer();
		}
	}
}