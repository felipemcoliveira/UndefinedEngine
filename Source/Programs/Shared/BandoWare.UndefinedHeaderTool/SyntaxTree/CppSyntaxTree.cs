namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public interface ISourceFile
{
}

public class CppSyntaxTree
{
   public Node RootNode { get; }

   private SourceFileTextPositionMap m_SourceFileTextPositionMap;

   internal CppSyntaxTree(Node rootNode, SourceFileTextPositionMap sourceFileTextPositionMap)
   {
      RootNode = rootNode;
      m_SourceFileTextPositionMap = sourceFileTextPositionMap;
   }

   /// <returns>Returns <c>null</c> if no header macro was found in the source
   ///    file.</returns>
   public static CppSyntaxTree? Parse(string sourceFileText, string sourceFilePath)
   {
      SourceFileTextPositionMap sourceFileTextPositionMap = new();

      // preprocess the source file
      SourceFileContentPreprocessor preprocessor = new(sourceFileText, sourceFilePath, sourceFileTextPositionMap);
      SourceFilePreprocessResult preprocessResult = preprocessor.Preprocess();

      // tokenize the preprocessed source file
      Tokenizer tokenizer = new(preprocessResult.ProcessedSourceFileText, sourceFilePath, sourceFileTextPositionMap);
      TokenizeResult tokenizeResult = tokenizer.Tokenize();

      // if no header macro was found, return null
      if (!tokenizeResult.HasHeaderMacro)
      {
         return null;
      }

      // parse tokens into syntax tree
      SyntaxParser parser = new(tokenizeResult, sourceFileTextPositionMap);
      RootNode rootNode = parser.Parse();

      return new CppSyntaxTree(rootNode, preprocessResult.PositionMap);
   }

   public void GetLineAndColumn(SourceFileTextPosition position, out int line, out int column)
   {
      m_SourceFileTextPositionMap.GetLineAndColumn(position, out line, out column);
   }
}
