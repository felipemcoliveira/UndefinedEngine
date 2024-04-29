using System.Diagnostics;

using Range = UndefinedCore.Range;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UENUM {Identifier,nq}")]
public class CppEnumItemNode
   (
      Range tokensRange,
      CppToken identifierToken,
      CppLexicalAnalysis lexicalAnalysis
   )
   : CppSyntaxNode(tokensRange, lexicalAnalysis)
{
   public CppToken IdentifierToken { get; } = identifierToken;

   public string Identifier
   {
      get
      {
         string identifier = LexicalAnalysis.GetTokenValue(IdentifierToken);
         CppEnumNode enumNode = (CppEnumNode)Parent!;

         Debug.Assert(enumNode != null);

         return $"{enumNode.Identifier}::{identifier}";
      }
   }
}
