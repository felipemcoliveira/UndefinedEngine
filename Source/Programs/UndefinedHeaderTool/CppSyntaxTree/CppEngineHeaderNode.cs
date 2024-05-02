using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("{Name,nq}")]
public class CppEngineHeaderNode(StringView type) : CppSyntaxNode
{
   public StringView Type { get; private set; } = type;
}
