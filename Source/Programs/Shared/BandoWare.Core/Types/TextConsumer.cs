using System;
using System.Diagnostics.CodeAnalysis;

namespace BandoWare.Core;

public enum IdentifierStyle
{
   AlphanumericWithUnderscore,
   AlphanumericWithDash,
   Alphanumeric,
}

public ref struct TextConsumer
{
   public readonly bool IsEndOfText => Position >= Chars.Length;

   public ref int Position;
   public ReadOnlySpan<char> Chars;

   public TextConsumer(ref int position, ReadOnlySpan<char> chars)
   {
      Position = ref position;
      Chars = chars;
   }

   public bool TryConsumeIdentifier(IdentifierStyle style = IdentifierStyle.AlphanumericWithUnderscore)
   {
      return TryConsumeIdentifier(out _, style);
   }

   public bool TryConsumeIdentifier([NotNullWhen(true)] out string? identifier, IdentifierStyle style = IdentifierStyle.AlphanumericWithUnderscore)
   {
      return style switch
      {
         IdentifierStyle.AlphanumericWithUnderscore => TryConsumeIdentifierAlphaNumericWithUnderscore(out identifier),
         IdentifierStyle.AlphanumericWithDash => TryConsumeIdentifierAlphaNumericWithDash(out identifier),
         IdentifierStyle.Alphanumeric => TryConsumeIdentifierAlphanumeric(out identifier),
         _ => throw new ArgumentOutOfRangeException(nameof(style)),
      };
   }

   private bool TryConsumeIdentifierAlphaNumericWithUnderscore([NotNullWhen(true)] out string? identifier)
   {
      if (IsEndOfText)
      {
         identifier = null;
         return false;
      }

      int start = Position;
      if (!char.IsLetter(Chars[Position]) && Chars[Position] != '_')
      {
         identifier = null;
         return false;
      }

      while (Position < Chars.Length && (char.IsLetterOrDigit(Chars[Position]) || Chars[Position] == '_'))
      {
         Position++;
      }

      identifier = Chars[start..Position].ToString();
      return true;
   }

   private bool TryConsumeIdentifierAlphaNumericWithDash([NotNullWhen(true)] out string? identifier)
   {
      if (IsEndOfText)
      {
         identifier = null;
         return false;
      }

      int start = Position;
      if (!char.IsLetter(Chars[Position]))
      {
         identifier = null;
         return false;
      }

      while (Position < Chars.Length && (char.IsLetterOrDigit(Chars[Position]) || Chars[Position] == '-'))
      {
         Position++;
      }

      identifier = Chars[start..Position].ToString();
      return true;
   }

   private bool TryConsumeIdentifierAlphanumeric([NotNullWhen(true)] out string? identifier)
   {
      if (IsEndOfText)
      {
         identifier = null;
         return false;
      }

      int start = Position;
      if (!char.IsLetter(Chars[Position]))
      {
         identifier = null;
         return false;
      }

      while (Position < Chars.Length && char.IsLetterOrDigit(Chars[Position]))
      {
         Position++;
      }

      identifier = Chars[start..Position].ToString();
      return true;
   }

   public bool TryConsume(ReadOnlySpan<char> value, StringComparison stringComparison = StringComparison.Ordinal)
   {
      if (Chars.Length - Position < value.Length)
      {
         return false;
      }

      ReadOnlySpan<char> span = Chars.Slice(Position, value.Length);
      if (span.Equals(value, stringComparison))
      {
         Position += value.Length;
         return true;
      }

      return false;
   }

   public bool TryConsume(char value)
   {
      if (Chars.Length - Position < 1)
      {
         return false;
      }

      if (Chars[Position] == value)
      {
         Position++;
         return true;
      }

      return false;
   }

   public void Expect(ReadOnlySpan<char> value, StringComparison stringComparison = StringComparison.Ordinal)
   {
      if (!TryConsume(value, stringComparison))
      {
         throw new InvalidOperationException($"Expected '{value.ToString()}' at position {Position}");
      }
   }
}
