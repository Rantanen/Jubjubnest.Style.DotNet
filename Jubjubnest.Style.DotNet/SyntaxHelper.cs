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
	/// <summary>
	/// Helper for syntax analysis related functionality.
	/// </summary>
	internal class SyntaxHelper
	{
		/// <summary>
		/// Get the type name for the syntax node.
		/// </summary>
		/// <param name="node">Declaration syntax node.</param>
		/// <returns>Declaration type name.</returns>
		public static string GetItemType( SyntaxNode node )
		{
			// Switch based on the syntax kind.
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

		/// <summary>
		/// Get the identifier for the syntax node.
		/// </summary>
		/// <param name="node">Syntax node to get the identifier for.</param>
		/// <returns>Identifier token.</returns>
		public static SyntaxToken GetIdentifier( SyntaxNode node )
		{
			// Switch based on the syntax node.
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

		/// <summary>
		/// Get the string display length.
		/// </summary>
		/// <param name="text">Text to resolve the length for.</param>
		/// <returns>String length assuming tab = 4 spaces.</returns>
		public static int GetTextLength( string text )
		{
			// Delegate.
			int charCount = 0;
			return GetTextLengthWith120Treshold( text, out charCount );
		}

		/// <summary>
		/// Get the string display length.
		/// </summary>
		/// <param name="text">Text to resolve the length for.</param>
		/// <param name="tresholdCharCount">Column of the character exceeding the 120-char limit.</param>
		/// <returns>String length assuming tab = 4 spaces.</returns>
		public static int GetTextLengthWith120Treshold(
			string text,
			out int tresholdCharCount )
		{
			// Calculate the text length.
			int length = 0;
			tresholdCharCount = 0;
			foreach( var c in text )
			{
				// Count this in the treshold if we're still below the length.
				if( length < 120 )
					tresholdCharCount++;

				// Figure out whether this is a multi-width display character.
				if( c == '\t' )
				{
					// Tab should end up at the next multiple of 4.
					// Add 4 to the length subtracted by the mod 4 to ensure the
					// final stays as a multiple of 4. If length is multiple of 4,
					// the remainder is 0 which causes the full + 4 on length.
					length += 4 - length % 4;
				}
				else
				{
					// Single-width character.
					length += 1;
				}
			}

			// If we ended up with less than 120 chars, reset the treshold to -1.
			if( length <= 120 )
				tresholdCharCount = -1;

			// Return.
			return length;
		}
	}
}
