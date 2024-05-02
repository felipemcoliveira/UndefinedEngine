using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UCLASS {Name,nq}")]
public class CppClassNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
