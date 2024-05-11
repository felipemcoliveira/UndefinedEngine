using BandoWare.Core;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class HeaderSpecifierNode(StringView name) : Node
{
   public StringView Name { get; } = name;
}