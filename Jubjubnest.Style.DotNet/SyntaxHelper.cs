using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jubjubnest.Style.DotNet
{
	internal class SyntaxHelper
	{
		public static string GetItemType( SyntaxNode node )
		{
			switch( node.Kind() )
			{
				case SyntaxKind.InterfaceDeclaration:
					return "interface";
				case SyntaxKind.ClassDeclaration:
					return "class";
				case SyntaxKind.StructDeclaration:
					return "struct";
				case SyntaxKind.EnumDeclaration:
					return "enum";
				case SyntaxKind.MethodDeclaration:
					return "method";
				case SyntaxKind.PropertyDeclaration:
					return "property";
				case SyntaxKind.FieldDeclaration:
					return "field";
				case SyntaxKind.EnumMemberDeclaration:
					return "enum value";
				default:
					return "<unknown item>";
			}
		}

		public static SyntaxToken GetIdentifier( SyntaxNode node )
		{
			switch( node.Kind() )
			{
				case SyntaxKind.InterfaceDeclaration:
					return ( ( InterfaceDeclarationSyntax )node ).Identifier;
				case SyntaxKind.ClassDeclaration:
					return ( ( ClassDeclarationSyntax )node ).Identifier;
				case SyntaxKind.StructDeclaration:
					return ( ( StructDeclarationSyntax )node ).Identifier;
				case SyntaxKind.EnumDeclaration:
					return ( ( EnumDeclarationSyntax )node ).Identifier;
				case SyntaxKind.MethodDeclaration:
					return ( ( MethodDeclarationSyntax )node ).Identifier;
				case SyntaxKind.PropertyDeclaration:
					return ( ( PropertyDeclarationSyntax )node ).Identifier;
				case SyntaxKind.FieldDeclaration:
					return ( ( FieldDeclarationSyntax )node ).Declaration.Variables.Single().Identifier;
				case SyntaxKind.EnumMemberDeclaration:
					return ( ( EnumDeclarationSyntax )node ).Identifier;
				default:
					return default( SyntaxToken );
			}
		}
	}
}
