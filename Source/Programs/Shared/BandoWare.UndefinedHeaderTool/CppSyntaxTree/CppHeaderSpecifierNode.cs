using BandoWare.Core;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class CppHeaderSpecifierNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; } = name;
}