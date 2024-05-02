using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class LexicalAnalysis(List<Token> tokens, List<int> engineHeaderTokenIndices, string sourceCode)
{
   public string SourceCode { get; } = sourceCode;
   public List<Token> Tokens { get; } = tokens;
   public List<int> EngineHeaderTokenIndices { get; } = engineHeaderTokenIndices;
}
