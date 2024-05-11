using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public record struct TokenizeResult(List<Token> Tokens, List<int> EngineHeaderTokenIndices, SourceFile SourceFile)
{
   public bool HasHeader => EngineHeaderTokenIndices.Count > 0;
}

