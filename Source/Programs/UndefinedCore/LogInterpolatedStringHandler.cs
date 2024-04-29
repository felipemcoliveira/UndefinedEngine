using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace UndefinedCore;

[InterpolatedStringHandler]
public ref struct LogInterpolatedStringHandler
{
   unsafe class AppendFunctionLookUp<T>
   {
      public static delegate*<StringBuilder, T, void> Function;
   }

   StringBuilder? m_Builder;

   static LogInterpolatedStringHandler()
   {
      static void AppendInt(StringBuilder stringBuilder, int v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendUInt(StringBuilder stringBuilder, uint v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendLong(StringBuilder stringBuilder, long v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendULong(StringBuilder stringBuilder, ulong v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendShort(StringBuilder stringBuilder, short v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendUShort(StringBuilder stringBuilder, ushort v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendByte(StringBuilder stringBuilder, byte v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendSByte(StringBuilder stringBuilder, sbyte v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendFloat(StringBuilder stringBuilder, float v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendDouble(StringBuilder stringBuilder, double v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendDecimal(StringBuilder stringBuilder, decimal v)
      {
         stringBuilder?.Append(v);
      }

      static void AppendBool(StringBuilder stringBuilder, bool v)
      {
         stringBuilder?.Append(v);
      }

      unsafe
      {
         AppendFunctionLookUp<int>.Function = &AppendInt;
         AppendFunctionLookUp<uint>.Function = &AppendUInt;
         AppendFunctionLookUp<long>.Function = &AppendLong;
         AppendFunctionLookUp<ulong>.Function = &AppendULong;
         AppendFunctionLookUp<short>.Function = &AppendShort;
         AppendFunctionLookUp<ushort>.Function = &AppendUShort;
         AppendFunctionLookUp<byte>.Function = &AppendByte;
         AppendFunctionLookUp<sbyte>.Function = &AppendSByte;
         AppendFunctionLookUp<float>.Function = &AppendFloat;
         AppendFunctionLookUp<double>.Function = &AppendDouble;
         AppendFunctionLookUp<decimal>.Function = &AppendDecimal;
         AppendFunctionLookUp<bool>.Function = &AppendBool;
      }
   }

   public LogInterpolatedStringHandler(int literalLength, int _, ScopedLogger logger, LogLevel logLevel)
   {
      m_Builder = logger.UnderlyingLogger != null && logLevel >= logger.UnderlyingLogger.LogLevel ? new(literalLength) : null;
   }

   public LogInterpolatedStringHandler(int literalLength, int _, ILogger logger, LogLevel logLevel)
   {
      m_Builder = logLevel >= logger.LogLevel ? new(literalLength) : null;
   }

   public void AppendLiteral(string s)
   {
      m_Builder?.Append(s);
   }

   public unsafe void AppendFormatted<T>(T t)
   {
      if (m_Builder == null)
      {
         return;
      }

      delegate*<StringBuilder, T, void> fn = AppendFunctionLookUp<T>.Function;

      if (fn == null)
      {
         m_Builder.Append(t);
         return;
      }

      fn(m_Builder, t);
   }

   internal string GetFormattedText()
   {
      Debug.Assert(m_Builder != null);
      return m_Builder!.ToString();
   }
}
