using BandoWare.Core;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class HeaderSpecifierNode(StringView name) : SyntaxNode
{
   public StringView Name { get; } = name;
}