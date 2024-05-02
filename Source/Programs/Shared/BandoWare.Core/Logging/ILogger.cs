using System;
using System.Runtime.CompilerServices;

namespace BandoWare.Core;

public interface ILogger
{
   public LogLevel LogLevel { get; }

   /// <summary>
   /// Logs an exception with optional message, automatically capturing caller information.
   /// </summary>
   /// <param name="scope">The scope of the log.</param>
   /// <param name="logLevel">The severity level of the log.</param>
   /// <param name="exception">The exception to log.</param>
   /// <param name="sourceFilePath">Automatically captured path of the source file calling this method.</param>
   /// <param name="sourceFileLine">Automatically captured line number in the source file calling this method.</param>
   /// <param name="memberName">Automatically captured name of the method calling this log method.</param>
   /// <remarks>
   /// The exception message will be used as the log message.
   /// </remarks>
   /// </summary>
   void LogException
   (
      in LogScope scope,
      LogLevel logLevel,
      Exception exception,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      Log(scope, logLevel, exception.Message, exception, sourceFilePath, sourceFileLine, memberName);
   }

   /// <summary>
   /// Logs a message with optional exception information, automatically capturing caller information.
   /// </summary>
   /// <param name="scope">The scope of the log.</param>
   /// <param name="logLevel">The severity level of the log.</param>
   /// <param name="message">The log message, supporting interpolated strings.</param>
   /// <param name="exception">Optional. The exception related to the log entry.</param>
   /// <param name="sourceFilePath">Automatically captured path of the source file calling this method.</param>
   /// <param name="sourceFileLine">Automatically captured line number in the source file calling this method.</param>
   /// <param name="memberName">Automatically captured name of the method calling this log method.</param>
   void Log
   (
      in LogScope scope,
      LogLevel logLevel,
      [InterpolatedStringHandlerArgument("", nameof(logLevel))] LogInterpolatedStringHandler message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   );

   /// <summary>
   /// Logs a message with optional exception information, automatically capturing caller information.
   /// </summary>
   /// <param name="scope">The scope of the log.</param>
   /// <param name="logLevel">The severity level of the log.</param>
   /// <param name="message">The log message.</param>
   /// <param name="exception">Optional. The exception related to the log entry.</param>
   /// <param name="sourceFilePath">Automatically captured path of the source file calling this method.</param>
   /// <param name="sourceFileLine">Automatically captured line number in the source file calling this method.</param>
   /// <param name="memberName">Automatically captured name of the method calling this log method.</param>
   void Log
   (
      in LogScope scope,
      LogLevel logLevel,
      string message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   );
}
