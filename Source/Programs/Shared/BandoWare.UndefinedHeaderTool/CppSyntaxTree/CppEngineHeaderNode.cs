using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{Name,nq}")]
public class CppEngineHeaderNode(StringView type) : CppSyntaxNode
{
   public StringView Type { get; private set; } = type;
}
