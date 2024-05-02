using System.Runtime.CompilerServices;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal static class NumericLiteralLexicalAnalysis
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsume(LexicalAnalyzer analyzer)
   {
      return TryConsumeHexadecimalLiteral(analyzer)
         || TryConsumeBinaryLiteral(analyzer)
         || TryConsumeOctalLiteral(analyzer)
         || TryConsumeDecimalLiteral(analyzer);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeDecimalLiteral(LexicalAnalyzer analyzer)
   {
      int start = analyzer.Position;
      bool isFloating = false;
      if (!analyzer.TryConsumeDigit(10) && !(isFloating = analyzer.TryConsume('.') && analyzer.TryConsumeDigit(10)))
      {
         analyzer.Position = start;
         return false;
      }

      ConsumeDigits(analyzer, 10);

      if (!isFloating && analyzer.TryConsume('.'))
      {
         ConsumeDigits(analyzer, 10);
      }

      TryConsumeExponent(analyzer);
      analyzer.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeOctalLiteral(LexicalAnalyzer analyzer)
   {
      int start = analyzer.Position;
      if (!analyzer.TryConsume('0') || !analyzer.TryConsumeDigit(8))
      {
         analyzer.Position = start;
         return false;
      }

      ConsumeDigits(analyzer, 8);
      if (!analyzer.IsEndOfFile && LexicalAnalyzer.AsciiHexDigitToInt(analyzer.CurrentCharacter) >= 8)
      {
         throw new IllFormedCodeException(analyzer.Position, "Invalid digit in octal constant.");
      }

      if (analyzer.TryConsume('.'))
      {
         throw new IllFormedCodeException(analyzer.Position, "Invalid prefix \"0\" for floating constant.");
      }

      TryConsumeExponent(analyzer);
      analyzer.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeBinaryLiteral(LexicalAnalyzer analyzer)
   {
      int start = analyzer.Position;
      if (!analyzer.TryConsumeIgnoreCase("0b") && !analyzer.TryConsumeDigit(2))
      {
         analyzer.Position = start;
         return false;
      }

      ConsumeDigits(analyzer, 2);

      if (!analyzer.IsEndOfFile && LexicalAnalyzer.AsciiHexDigitToInt(analyzer.CurrentCharacter) >= 2)
      {
         throw new IllFormedCodeException(analyzer.Position, "Invalid digit in binary constant.");
      }

      if (analyzer.TryConsume('.') || analyzer.TryConsume('e'))
      {
         throw new IllFormedCodeException(analyzer.Position, "Invalid prefix \"0b\" for floating constant.");
      }

      analyzer.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeHexadecimalLiteral(LexicalAnalyzer ctx)
   {
      int start = ctx.Position;
      bool isFloating = false;
      if (!ctx.TryConsumeIgnoreCase("0x") || !ctx.TryConsumeDigit(16) && !(isFloating = ctx.TryConsume('.') && ctx.TryConsumeDigit(16)))
      {
         ctx.Position = start;
         return false;
      }

      ConsumeDigits(ctx, 16);

      if (!isFloating && ctx.TryConsume('.'))
      {
         isFloating = true;
         ConsumeDigits(ctx, 16);
      }

      if (isFloating)
      {
         if (!ctx.TryConsumeIgnoreCase('p'))
         {
            throw new IllFormedCodeException(ctx.Position, "Hexadecimal floating constants require an exponent.");
         }

         ctx.TryConsumeSingleNumericSign();
         ConsumeDigits(ctx, 10);
      }

      ctx.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool TryConsumeExponent(LexicalAnalyzer analyzer)
   {
      if (!analyzer.TryConsumeIgnoreCase('e'))
      {
         return false;
      }

      analyzer.TryConsumeSingleNumericSign();

      if (!analyzer.TryConsumeDigit(10))
      {
         throw new IllFormedCodeException(analyzer.Position, "Exponent has no digits.");
      }

      ConsumeDigits(analyzer, 10);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void ConsumeDigits(LexicalAnalyzer analyzer, int numericBase)
   {
      while (analyzer.TryConsumeDigit(numericBase))
      {
      }
   }
}
