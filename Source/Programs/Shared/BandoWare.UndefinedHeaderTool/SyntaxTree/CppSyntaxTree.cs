using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("SyntaxTree")]
public class CppSyntaxTree : Node
{
   public ReadOnlyCollection<Token> Tokens { get; }

   private TokenizeResult m_TokenizeResult;

   /// <returns>Returns <c>null</c> if no header was found in the source file.
   ///    </returns>
   public static CppSyntaxTree? Parse(SourceFile sourceFile)
   {
      Tokenizer tokenizer = new(sourceFile);
      TokenizeResult tokenizeResult = tokenizer.Tokenize();

      if (!tokenizeResult.HasHeader)
      {
         return null;
      }

      SyntaxParser parser = new(tokenizeResult);
      return parser.Parse();
   }
}
