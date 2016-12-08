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
	public class NamingTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestNamespaceWithCamelCaseName()
		{
			var code = @"namespace foo { }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 11, NamingAnalyzer.NameNamespacesWithPascalCasing, "foo" ) );
		}

		[TestMethod]
		public void TestNamespacesContainingOneWithCamelCaseName()
		{
			var code = @"namespace Foo.bar.Foobar { }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 15, NamingAnalyzer.NameNamespacesWithPascalCasing, "bar" ) );
		}

		[TestMethod]
		public void TestNumericNameWithCamelCase()
		{
			VerifyCSharpDiagnostic(
					@"namespace Foo1Bar { class Foo2Bar { } }",
					new TestEnvironment { FileName = "Foo2Bar" } ) ;
		}

		[TestMethod]
		public void TestClassWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { class bar { } }",
					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 23, NamingAnalyzer.NameTypesWithPascalCasing, "class", "bar" ) );
		}

		[TestMethod]
		public void TestEnumWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { enum bar { } }",
					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 22, NamingAnalyzer.NameTypesWithPascalCasing, "enum", "bar" ) );
		}

		[TestMethod]
		public void TestInterfaceWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { interface bar { } }",
					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 27, NamingAnalyzer.NameTypesWithPascalCasing, "interface", "bar" ),
					Warning( 1, 27, NamingAnalyzer.NameInterfacesWithIPrefix, "bar" ) );
		}

		[TestMethod]
		public void TestEnumValueWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { enum Bar { foo } }",
					new TestEnvironment { FileName = "Bar.cs" },
					Warning( 1, 28, NamingAnalyzer.NameEnumValuesWithPascalCase, "foo" ) );
		}


		[TestMethod]
		public void TestInterfaceWithoutIPrefix()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { interface Bar { } }",
					new TestEnvironment { FileName = "Bar.cs" },
					Warning( 1, 27, NamingAnalyzer.NameInterfacesWithIPrefix, "Bar" ) );
		}

		[TestMethod]
		public void TestExceptionWithoutExceptionSuffix()
		{

			VerifyCSharpDiagnostic(
					@"namespace Foo { class Foo : Exception { } }",
					new TestEnvironment { FileName = "Foo.cs" },
					Warning( 1, 23, NamingAnalyzer.NameExceptionsWithExceptionSuffix, "Foo" ) );
		}

		[TestMethod]
		public void TestProperNaming()
		{
			VerifyCSharpDiagnostic(
				@"namespace Foo.Bar {

					class Bar {
						public Bar() {}
						public void Method() {}
						public string Property { get; set; }

						private string privateField;
						private readonly string READONLY_FIELD = null
						private const int CONST_FIELD = 1;
					}
				}",
				new TestEnvironment { FileName = "Bar.cs" } );

			VerifyCSharpDiagnostic(
				@"namespace Foo.Bar {
					interface IBar { }
				}",
				new TestEnvironment { FileName = "IBar.cs" } );

			VerifyCSharpDiagnostic(
				@"namespace Foo.Bar {
					enum Foo { }
				}",
				new TestEnvironment { FileName = "Foo.cs" } );
		}

		[TestMethod]
		public void TestWrongFilename()
		{
			VerifyCSharpDiagnostic(
					@"namespace TestProject { class Bar { } }",
					new TestEnvironment { FileName = "File" },
					Warning( 1, 25, NamingAnalyzer.NameFilesAccordingToTypeNames, "Bar", "Bar.cs" ) );
		}

		[TestMethod]
		public void TestWrongFilenameInDirectory()
		{
			VerifyCSharpDiagnostic(
					@"namespace TestProject { class Bar { } }",
					new TestEnvironment { FileName = @"Path\File" },
					Warning( 1, 25, NamingAnalyzer.NameFilesAccordingToTypeNames, "Bar", "Bar.cs" ) );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new JubjubnestStyleDotNetCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
            return new NamingAnalyzer();
		}
	}
}