using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BandoWare.Core;

[DebuggerDisplay("{ToString(),nq}")]
public struct StringView : IEquatable<StringView>, IComparable<StringView>
{
   public static readonly StringView Empty = new(string.Empty);

   public ReadOnlyMemory<char> Memory { get; private set; }

   public readonly ReadOnlySpan<char> Span => Memory.Span;

   public readonly int Length => Memory.Length;

   public readonly char this[int index] => Span[index];

   public readonly char this[Index index] => Span[index];

   public readonly StringView this[Range range] => new(Memory[range]);

   public StringView(StringView stringView)
   {
      Memory = stringView.Memory;
   }

   public StringView(StringView stringView, int start)
   {
      Memory = stringView.Memory[start..];
   }

   public StringView(StringView stringView, int start, int length)
   {
      Memory = stringView.Memory.Slice(start, length);
   }

   public StringView(ReadOnlyMemory<char> memory)
   {
      Memory = memory;
   }

   public override int GetHashCode()
   {
      return string.GetHashCode(Span);
   }

   public override readonly bool Equals(object? obj)
   {
      return obj is StringView view && Equals(view);
   }

   public bool Equals(ReadOnlySpan<char> span)
   {
      return Span.SequenceEqual(span);
   }

   public readonly bool Equals(ReadOnlySpan<char> span, StringComparison stringComparison)
   {
      return Span.Equals(span, stringComparison);
   }

   public readonly bool Equals(StringView other)
   {
      return Span.SequenceEqual(other.Span);
   }

   public readonly bool Equals(StringView other, StringComparison stringComparison)
   {
      return Span.Equals(other.Span, stringComparison);
   }

   public readonly StringView Slice(int start)
   {
      return new StringView(this, start);
   }

   public readonly StringView Slice(int start, int length)
   {
      return new StringView(this, start, length);
   }

   public static bool operator ==(StringView left, StringView right)
   {
      return left.Equals(right);
   }

   public static bool operator !=(StringView left, StringView right)
   {
      return !left.Equals(right);
   }

   public static implicit operator StringView(string value)
   {
      return new StringView(value.AsMemory());
   }

   public static implicit operator ReadOnlyMemory<char>(StringView view)
   {
      return view.Memory;
   }

   public static implicit operator ReadOnlySpan<char>(StringView view)
   {
      return view.Span;
   }

   public static implicit operator string(StringView view)
   {
      return view.Span.ToString();
   }

   public static implicit operator StringView(ReadOnlyMemory<char> memory)
   {
      return new StringView(memory);
   }

   public static implicit operator StringView(ReadOnlySpan<char> span)
   {
      return new StringView(span.ToArray());
   }

   public override readonly string ToString()
   {
      return Span.ToString();
   }

   public readonly int CompareTo(StringView other)
   {
      return Span.CompareTo(other.Span, default);
   }

   public readonly int CompareTo(StringView other, StringComparison stringComparison)
   {
      return Span.CompareTo(other.Span, stringComparison);
   }

   public readonly ReadOnlySpan<char>.Enumerator GetEnumerator()
   {
      return Span.GetEnumerator();
   }
}

public class StringViewComparer : IComparer<StringView>
{
   public StringComparison StringComparison { get; }

   public StringViewComparer(StringComparison stringComparison)
   {
      StringComparison = stringComparison;
   }

   public int Compare(StringView x, StringView y)
   {
      return x.CompareTo(y, StringComparison);
   }
}