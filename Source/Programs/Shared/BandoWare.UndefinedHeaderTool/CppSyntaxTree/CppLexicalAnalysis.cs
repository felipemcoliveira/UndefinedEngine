using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class CppLexicalAnalysis(List<CppToken> tokens, List<int> engineHeaderTokenIndices, string sourceCode)
{
   public string SourceCode { get; } = sourceCode;
   public List<CppToken> Tokens { get; } = tokens;
   public List<int> EngineHeaderTokenIndices { get; } = engineHeaderTokenIndices;
}
