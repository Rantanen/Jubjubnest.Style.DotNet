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

			VerifyCSharpDiagnostic(

					@"namespace TestProject.foo { }",

					new TestEnvironment { FileName = "foo/Test.cs" },
					Warning( 1, 23, NamingAnalyzer.NameNamespacesWithPascalCasing, "foo" ) );
		}

		[TestMethod]
		public void TestNamespacesContainingOneWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(

					@"namespace TestProject.bar.Foobar { }",

					new TestEnvironment { FileName = "bar/Foobar/Test.cs" },
					Warning( 1, 23, NamingAnalyzer.NameNamespacesWithPascalCasing, "bar" ) );
		}

		[TestMethod]
		public void TestNumericNameWithCamelCase()
		{
			VerifyCSharpDiagnostic(

					@"namespace TestProject.Foo1Bar { class Foo2Bar { } }",

					new TestEnvironment { FileName = "Foo1Bar/Foo2Bar.cs" } ) ;
		}

		[TestMethod]
		public void TestClassWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(

					@"namespace TestProject { class bar { } }",

					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 31, NamingAnalyzer.NameTypesWithPascalCasing, "class", "bar" ) );
		}

		[TestMethod]
		public void TestEnumWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(

					@"namespace TestProject { enum bar { } }",

					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 30, NamingAnalyzer.NameTypesWithPascalCasing, "enum", "bar" ) );
		}

		[TestMethod]
		public void TestEventWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(

					@"namespace TestProject { class Bar { public event EventHandler eventName; } }",

					new TestEnvironment { FileName = "Bar.cs" },
					Warning( 1, 63, NamingAnalyzer.NameEventsWithPascalCase, "eventName" ) );
		}

		[TestMethod]
		public void TestInterfaceWithCamelCaseName()
		{

			VerifyCSharpDiagnostic(

					@"namespace TestProject { interface bar { } }",

					new TestEnvironment { FileName = "bar.cs" },
					Warning( 1, 35, NamingAnalyzer.NameTypesWithPascalCasing, "interface", "bar" ),
					Warning( 1, 35, NamingAnalyzer.NameInterfacesWithIPrefix, "bar" ) );
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
						public string XSingleCapitalCharacter { get; set; }
						public string SingleXCapitalCharacter { get; set; }
						public string SingleCapitalCharacterX { get; set; }

						private string privateField;
						private string singleXCapitalCharacter;
						private string singleCapitalCharacterX;
						private readonly string readOnlyField = null
						private const int ConstField = 1;

						public event EventHandler FooBar;
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
		public void TestAbbreviationFails()
		{
			VerifyCSharpDiagnostic(
				@"class XYPascalCase {
						public void PascalXYCase() {}
						public void XYZPascalCase() {}
						public void PascalXYZCase() {}
						public void PascalCaseXY() {}

						private string camelXYCase;
						private string camelXYZCase;
						private string camelCaseXY;
					}
				}",
				new TestEnvironment { FileName = "XYPascalCase.cs" },
				Warning( 1, 7, NamingAnalyzer.NameTypesWithPascalCasing, "class", "XYPascalCase" ),
				Warning( 2, 19, NamingAnalyzer.NameMethodsWithPascalCasing, "PascalXYCase" ),
				Warning( 3, 19, NamingAnalyzer.NameMethodsWithPascalCasing, "XYZPascalCase" ),
				Warning( 4, 19, NamingAnalyzer.NameMethodsWithPascalCasing, "PascalXYZCase" ),
				Warning( 5, 19, NamingAnalyzer.NameMethodsWithPascalCasing, "PascalCaseXY" ),
				Warning( 7, 22, NamingAnalyzer.NameFieldsWithCamelCase, "camelXYCase" ),
				Warning( 8, 22, NamingAnalyzer.NameFieldsWithCamelCase, "camelXYZCase" ),
				Warning( 9, 22, NamingAnalyzer.NameFieldsWithCamelCase, "camelCaseXY" ) );
		}

		[TestMethod]
		public void TestWrongFileName()
		{
			VerifyCSharpDiagnostic(

					@"namespace TestProject { class Bar { } }",

					new TestEnvironment { FileName = "File" },
					Warning( 1, 31, NamingAnalyzer.NameFilesAccordingToTypeNames, "Bar", "Bar.cs" ) );
		}

		[TestMethod]
		public void TestWrongFileNameInDirectory()
		{
			VerifyCSharpDiagnostic(

					@"namespace TestProject { class Bar { } }",

					new TestEnvironment { FileName = @"Path\File" },
					Warning( 1, 31, NamingAnalyzer.NameFilesAccordingToTypeNames, "Bar", "Bar.cs" ) );
		}

		[TestMethod]
		public void TestNamesEndingInLetterAndNumber()
		{
			VerifyCSharpDiagnostic( @"namespace SomethingA1 { class Bar { } }",
					new TestEnvironment { FileName = "Bar.cs" } );
		}

		[TestMethod, Ignore /* Folder name rules not implemented */ ]
		public void TestWrongFolderName()
		{
			VerifyCSharpDiagnostic(

					@"namespace TestProject.Folder { class Bar { } }",

					new TestEnvironment { FileName = "File" },
					Warning( 1, 11, NamingAnalyzer.NameFoldersAccordingToNamespaces, "TestProject.Folder", "Folder" ) );
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new NamingAnalyzer();
		}
	}
}