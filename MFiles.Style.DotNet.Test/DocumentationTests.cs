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
	public class DocumentationTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestDocumentationMissingOnInterface()
		{
			var code = @"namespace Foo { interface IBar { } }";

			VerifyCSharpDiagnostic( code, Warning( 1, 27, DocumentationAnalyzer.XmlDocumentEverythingWithSummary, "interface", "IBar" ) );
		}

		[TestMethod]
		public void TestDocumentationMissingOnClass()
		{
			var code = @"namespace Foo { class Bar { } }";

			VerifyCSharpDiagnostic( code, Warning( 1, 23, DocumentationAnalyzer.XmlDocumentEverythingWithSummary, "class", "Bar" ) );
		}

		[TestMethod]
		public void TestDocumentationMissingOnStruct()
		{
			var code = @"namespace Foo { struct Bar { } }";

			VerifyCSharpDiagnostic( code, Warning( 1, 24, DocumentationAnalyzer.XmlDocumentEverythingWithSummary, "struct", "Bar" ) );
		}

		[TestMethod]
		public void TestDocumentationMissingOnEnum()
		{
			var code = @"namespace Foo { enum Bar { } }";

			VerifyCSharpDiagnostic( code, Warning( 1, 22, DocumentationAnalyzer.XmlDocumentEverythingWithSummary, "enum", "Bar" ) );
		}


		[TestMethod]
		public void TestDocumentationExistsOnInterface()
		{
			var code = @"namespace Foo {
				/// <summary>Interface</summary>
				interface IBar { }
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationExistsOnClass()
		{
			var code = @"namespace Foo {
				/// <summary>Class</summary>
				class Bar { }
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationExistsOnStruct()
		{
			var code = @"namespace Foo {
				/// <summary>Struct</summary>
				struct Bar { }
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationExistsOnEnum()
		{
			var code = @"namespace Foo {
				/// <summary>Enum</summary>
				enum Bar { }
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationHasMultipleLines()
		{
			var code = @"namespace Foo {
				/// <summary>
				/// Struct
				/// </summary>
				struct Bar { }
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationMissingOnClassMethod()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					void Foobar() {}
				}
			}";

			VerifyCSharpDiagnostic( code, Warning( 4, 11, DocumentationAnalyzer.XmlDocumentEverythingWithSummary, "method", "Foobar" ) );
		}

		[TestMethod]
		public void TestDocumentationParamsMissingOnClassMethod()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					/// <summary>Foobar</summary>
					void Foobar( string foo ) {}
				}
			}";

			VerifyCSharpDiagnostic( code, Warning( 5, 26, DocumentationAnalyzer.XmlDocumentAllMethodParams, "foo" ) );
		}

		[TestMethod]
		public void TestDocumentationMismatchedParams()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					/// <summary>Foobar</summary>
					/// <param name=""foo"">param</param>
					void Foobar() {}
				}
			}";

			VerifyCSharpDiagnostic( code, Warning( 5, 17, DocumentationAnalyzer.XmlDocumentationNoMismatchedParam, "foo" ) );
		}

		[TestMethod]
		public void TestDocumentationEmptyContent()
		{
			var code = @"namespace Foo {
				/// <summary></summary>
				class Bar {
					/// <summary></summary>
					/// <param name=""foo""></param>
					void Foobar( string foo ) {}
				}
			}";

			VerifyCSharpDiagnostic( code,
					Warning( 2, 9, DocumentationAnalyzer.XmlDocumentNoEmptyContent, "summary" ),
					Warning( 4, 10, DocumentationAnalyzer.XmlDocumentNoEmptyContent, "summary" ),
					Warning( 5, 10, DocumentationAnalyzer.XmlDocumentNoEmptyContent, "param" ) );
		}

		[TestMethod]
		public void TestDocumentationParamsExistsOnClassMethod()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					/// <summary>Foobar</summary>
					/// <param name=""foo"">Foo</param>
					void Foobar( string foo ) {}
				}
			}";

			VerifyCSharpDiagnostic( code );
		}

		[TestMethod]
		public void TestDocumentationMissingReturnValue()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					/// <summary>Foobar</summary>
					string Foobar() {}
				}
			}";

			VerifyCSharpDiagnostic( code, Warning( 5, 13, DocumentationAnalyzer.XmlDocumentReturnValues, "Foobar" ) );
		}

		[TestMethod]
		public void TestDocumentationHasReturnValue()
		{
			var code = @"namespace Foo {
				/// <summary>Bar</summary>
				class Bar {
					/// <summary>Foobar</summary>
					/// <returns>String</returns>
					string Foobar() {}
				}
			}";

			VerifyCSharpDiagnostic( code );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new MFilesStyleDotNetCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new DocumentationAnalyzer();
		}
	}
}