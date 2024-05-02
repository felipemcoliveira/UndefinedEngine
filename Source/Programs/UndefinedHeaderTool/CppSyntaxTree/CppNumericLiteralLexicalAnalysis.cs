using System.Runtime.CompilerServices;

namespace UndefinedHeader.SyntaxTree;

internal static class CppNumericLiteralLexicalAnalysis
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsume(CppLexicalAnalyzer analyzer)
   {
      return TryConsumeHexadecimalLiteral(analyzer)
         || TryConsumeBinaryLiteral(analyzer)
         || TryConsumeOctalLiteral(analyzer)
         || TryConsumeDecimalLiteral(analyzer);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeDecimalLiteral(CppLexicalAnalyzer analyzer)
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
   public static bool TryConsumeOctalLiteral(CppLexicalAnalyzer analyzer)
   {
      int start = analyzer.Position;
      if (!analyzer.TryConsume('0') || !analyzer.TryConsumeDigit(8))
      {
         analyzer.Position = start;
         return false;
      }

      ConsumeDigits(analyzer, 8);
      if (!analyzer.IsEndOfFile && CppLexicalAnalyzer.AsciiHexDigitToInt(analyzer.CurrentCharacter) >= 8)
      {
         throw new CppIllFormedCodeException(analyzer.Position, "Invalid digit in octal constant.");
      }

      if (analyzer.TryConsume('.'))
      {
         throw new CppIllFormedCodeException(analyzer.Position, "Invalid prefix \"0\" for floating constant.");
      }

      TryConsumeExponent(analyzer);
      analyzer.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeBinaryLiteral(CppLexicalAnalyzer analyzer)
   {
      int start = analyzer.Position;
      if (!analyzer.TryConsumeIgnoreCase("0b") && !analyzer.TryConsumeDigit(2))
      {
         analyzer.Position = start;
         return false;
      }

      ConsumeDigits(analyzer, 2);

      if (!analyzer.IsEndOfFile && CppLexicalAnalyzer.AsciiHexDigitToInt(analyzer.CurrentCharacter) >= 2)
      {
         throw new CppIllFormedCodeException(analyzer.Position, "Invalid digit in binary constant.");
      }

      if (analyzer.TryConsume('.') || analyzer.TryConsume('e'))
      {
         throw new CppIllFormedCodeException(analyzer.Position, "Invalid prefix \"0b\" for floating constant.");
      }

      analyzer.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool TryConsumeHexadecimalLiteral(CppLexicalAnalyzer ctx)
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
            throw new CppIllFormedCodeException(ctx.Position, "Hexadecimal floating constants require an exponent.");
         }

         ctx.TryConsumeSingleNumericSign();
         ConsumeDigits(ctx, 10);
      }

      ctx.TryConsumeIdentifier(out _);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool TryConsumeExponent(CppLexicalAnalyzer analyzer)
   {
      if (!analyzer.TryConsumeIgnoreCase('e'))
      {
         return false;
      }

      analyzer.TryConsumeSingleNumericSign();

      if (!analyzer.TryConsumeDigit(10))
      {
         throw new CppIllFormedCodeException(analyzer.Position, "Exponent has no digits.");
      }

      ConsumeDigits(analyzer, 10);
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void ConsumeDigits(CppLexicalAnalyzer analyzer, int numericBase)
   {
      while (analyzer.TryConsumeDigit(numericBase))
      {
      }
   }
}
