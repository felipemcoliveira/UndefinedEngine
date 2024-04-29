using Range = UndefinedCore.Range;

namespace UndefinedHeader.SyntaxTree;

public class CppSpecifierNode
   (
      Range tokensRange,
      CppToken identifierToken,
      Range valueTokensRange,
      CppLexicalAnalysis lexicalAnalysis
   )
   : CppSyntaxNode(tokensRange, lexicalAnalysis)
{
   public CppToken IdentifierToken { get; } = identifierToken;
   public Range ValueTokensRange { get; } = valueTokensRange;

   public string Identifier => LexicalAnalysis.GetTokenValue(IdentifierToken);
}
