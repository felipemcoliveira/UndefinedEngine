using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("{Name,nq}")]
public class CppSpecifierNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
