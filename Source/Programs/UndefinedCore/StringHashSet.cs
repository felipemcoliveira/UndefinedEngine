using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UndefinedCore;

public class StringHashSet : IEnumerable<string>
{
   private Dictionary<int, string> m_Dictionary;
   private HashSet<char> m_CharSet;

   public StringHashSet()
   {
      m_Dictionary = [];
      m_CharSet = [];
   }

   public int Count => m_Dictionary.Count;

   public bool IsCharacterInSet(char c)
   {
      return m_CharSet.Contains(c);
   }

   public bool Add(string item)
   {
      int hashCode = item.GetHashCode();
      if (m_Dictionary.TryGetValue(hashCode, out string? value))
      {
         if (MemoryExtensions.Equals(value, item, StringComparison.Ordinal))
         {
            return false;
         }

         throw new InvalidOperationException("Hash collision.");
      }

      m_Dictionary.Add(hashCode, item);
      foreach (char c in item)
      {
         m_CharSet.Add(c);
      }

      return true;
   }

   public bool Contains(ReadOnlySpan<char> span, [NotNullWhen(true)] out string? str)
   {
      int hashCode = string.GetHashCode(span);
      if (m_Dictionary.TryGetValue(hashCode, out str))
      {
         return MemoryExtensions.Equals(str, span, StringComparison.Ordinal);
      }

      return false;
   }


   public bool Contains(ReadOnlySpan<char> span)
   {
      int hashCode = string.GetHashCode(span);
      if (m_Dictionary.TryGetValue(hashCode, out string? value))
      {
         return MemoryExtensions.Equals(value, span, StringComparison.Ordinal);
      }

      return false;
   }

   public bool Contains(string item)
   {
      int hashCode = string.GetHashCode(item);
      if (m_Dictionary.TryGetValue(hashCode, out string? value))
      {
         return string.Compare(item, value, StringComparison.Ordinal) == 0;
      }

      return false;
   }

   public IEnumerator<string> GetEnumerator()
   {
      return m_Dictionary.Values.GetEnumerator();
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
}
