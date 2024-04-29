namespace UndefinedHeader.SyntaxTree;

public class CppEngineHeaderNode(Range range, CppLexicalAnalysis lexicalAnalysis)
   : CppSyntaxNode(range, lexicalAnalysis)
{
   public bool TryGetSpecifier(string specifierName, out CppSpecifierNode? specifierNode)
   {
      foreach (CppSyntaxNode node in this)
      {
         if (node is CppSpecifierNode tmpSpecifierNode && tmpSpecifierNode.Identifier == specifierName)
         {
            specifierNode = tmpSpecifierNode;
            return true;
         }
      }

      specifierNode = null;
      return false;
   }
}
