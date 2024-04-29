using System.Diagnostics;

using Range = UndefinedCore.Range;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UENUM {Identifier,nq}")]
public class CppEnumNode
   (Range tokensRange, CppToken identifierToken, CppLexicalAnalysis lexicalAnalysis)
   : CppSyntaxNode(tokensRange, lexicalAnalysis)
{
   public CppToken IdentifierToken { get; } = identifierToken;

   public string Identifier => LexicalAnalysis.GetTokenValue(IdentifierToken);
}
