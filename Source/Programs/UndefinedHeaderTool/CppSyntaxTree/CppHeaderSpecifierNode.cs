using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

public class CppHeaderSpecifierNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; } = name;
}