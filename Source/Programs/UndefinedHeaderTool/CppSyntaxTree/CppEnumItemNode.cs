using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UENUM Item {Name,nq}")]
public class CppEnumItemNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
