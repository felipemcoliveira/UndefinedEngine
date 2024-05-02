using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

public class CppHeaderNode(StringView type) : CppSyntaxNode
{
   public StringView Type { get; } = type;

   public bool TryGetSpecifier(string specifierName, out CppHeaderSpecifierNode? specifierNode)
   {
      foreach (CppSyntaxNode node in this)
      {
         if (node is CppHeaderSpecifierNode tmpSpecifierNode && tmpSpecifierNode.Name == specifierName)
         {
            specifierNode = tmpSpecifierNode;
            return true;
         }
      }

      specifierNode = null;
      return false;
   }
}
