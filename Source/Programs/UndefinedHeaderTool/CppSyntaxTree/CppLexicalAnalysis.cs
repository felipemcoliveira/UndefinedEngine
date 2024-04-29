using System;
using System.Collections.Generic;

namespace UndefinedHeader.SyntaxTree;

public class CppLexicalAnalysis(List<CppToken> tokens, List<int> engineHeaderTokenIndices, string sourceCode)
{
   public string SourceCode { get; } = sourceCode;
   public List<CppToken> Tokens { get; } = tokens;
   public List<int> EngineHeaderTokenIndices { get; } = engineHeaderTokenIndices;

   public string GetTokenValue(int tokenIndex)
   {
      var token = Tokens[tokenIndex];
      return new string(SourceCode.AsSpan(token.StartPosition, token.Length));
   }

   public string GetTokenValue(CppToken token)
   {
      return new string(SourceCode.AsSpan(token.StartPosition, token.Length));
   }
}
