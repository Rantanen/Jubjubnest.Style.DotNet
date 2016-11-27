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
		public void TestClassWithCamelCaseName()
		{
			var code = @"namespace Foo { class bar { } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 23, NamingAnalyzer.NameTypesWithPascalCasing, "class", "bar" ) );
		}

		[TestMethod]
		public void TestEnumWithCamelCaseName()
		{
			var code = @"namespace Foo { enum bar { } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 22, NamingAnalyzer.NameTypesWithPascalCasing, "enum", "bar" ) );
		}

		[TestMethod]
		public void TestInterfaceWithCamelCaseName()
		{
			var code = @"namespace Foo { interface bar { } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 27, NamingAnalyzer.NameTypesWithPascalCasing, "interface", "bar" ),
					Warning( 1, 27, NamingAnalyzer.NameInterfacesWithIPrefix, "bar" ) );
		}

		[TestMethod]
		public void TestEnumValueWithCamelCaseName()
		{
			var code = @"namespace Foo { enum Bar { foo } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 28, NamingAnalyzer.NameEnumValuesWithPascalCase, "foo" ) );
		}


		[TestMethod]
		public void TestInterfaceWithoutIPrefix()
		{
			var code = @"namespace Foo { interface Bar { } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 27, NamingAnalyzer.NameInterfacesWithIPrefix, "Bar" ) );
		}

		[TestMethod]
		public void TestExceptionWithoutExceptionSuffix()
		{
			var code = @"namespace Foo { class Foo : Exception { } }";

			VerifyCSharpDiagnostic( code,
					Warning( 1, 23, NamingAnalyzer.NameExceptionsWithExceptionSuffix, "Foo" ) );
		}

		[TestMethod]
		public void TestProperNaming()
		{
			var code = @"
				namespace Foo.Bar {

					class Bar {
						public Bar() {}
						public void Method() {}
						public string Property { get; set; }
					}

					interface IBar { }
					enum Foo { }
				}";

			VerifyCSharpDiagnostic( code );
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