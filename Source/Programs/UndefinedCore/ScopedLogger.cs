using System;
using System.Runtime.CompilerServices;

namespace UndefinedCore;

public struct ScopedLogger
{
   public LogScope Scope { get; set; }
   public ILogger? UnderlyingLogger { get; set; }

   /// <inheritdoc cref="ILogger.LogException(in LogScope, LogLevel, Exception, string, int, string)"/>
   public readonly void LogException
   (
      LogLevel logLevel,
      Exception exception,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      UnderlyingLogger?.LogException(Scope, logLevel, exception, sourceFilePath, sourceFileLine, memberName);
   }

   /// <inheritdoc cref="ILogger.Log(in LogScope, LogLevel, LogInterpolatedStringHandler, Exception?, string, int, string)"/>
   public readonly void Log
   (
      LogLevel logLevel,
      [InterpolatedStringHandlerArgument("", nameof(logLevel))] LogInterpolatedStringHandler message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      UnderlyingLogger?.Log(Scope, logLevel, message, exception, sourceFilePath, sourceFileLine, memberName);
   }

   /// <inheritdoc cref="ILogger.Log(in LogScope, LogLevel, string, Exception?, string, int, string)"/>
   public readonly void Log
   (
      LogLevel logLevel,
      string message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      UnderlyingLogger?.Log(Scope, logLevel, message, exception, sourceFilePath, sourceFileLine, memberName);
   }
}
