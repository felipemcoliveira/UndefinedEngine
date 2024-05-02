using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UENUM {Name,nq}")]
public class CppEnumNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
