using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UndefinedCore;

/// <summary>
/// Provides logging functionality with support for console and file output.
/// </summary>
public class Logger : ILogger, IDisposable
{
   private readonly static char[] s_ConsoleOutputBuffer = new char[16 * 1024];
   private readonly static object s_SyncObject = new();

   private List<Log> m_ActiveLogList = [];
   private List<Log> m_WriteLogList = [];
   private bool m_IsDisposed;
   private Thread m_Thread;
   private BufferedStream? m_FileOutput;
   private FileStream? m_FileStream;
   private StringBuilder m_StringBuilder = new();
   private int m_ConsoleOutputBufferPosition = 0;

   /// <summary>
   /// Specifies the logging level. Logs below this level will be ignored.
   /// </summary>
   [CommandLine("-LogLevel", ValueUsage = "$EnumValues", Description = "Specifies the logging level. Logs below this level will be ignored.")]
   public LogLevel LogLevel { get; set; } = LogLevel.Info;

   /// <summary>
   /// Path to the log file. If specified, logs will be written to this file.
   /// </summary>
   [CommandLine("-LogOutput", ValueUsage = "<FilePath>", Description = "Path to the log file. If specified, logs will be written to this file.")]
   public string? LogFilePath { get; set; } = "/UndefinedHeaderTool.txt";

   /// <summary>
   /// Indicates whether timestamps should be included in each log entry.
   /// </summary>
   [CommandLine("-LogTimestamps", Description = "Indicates whether timestamps should be included in each log entry.")]
   public bool LogTimestamps { get; set; }

   /// <summary>
   /// Specifies the level of caller information to include in the log output.
   /// </summary>
   [CommandLine("-LogCallerInfo", ValueUsage = "$EnumValues", Description = "Specifies the level of caller information to include in the log output.")]
   public LogCallerInfo LogCallerInfo { get; set; } = LogCallerInfo.None;

   /// <summary>
   /// Indicates whether console output should use colors based on log level.
   /// </summary>
   [CommandLine("-LogUseConsoleColors", Description = "Indicates whether console output should use colors based on log level.")]
   public bool UseConsoleColors { get; set; }

   // <summary>
   /// Initializes a new instance of the Logger class, starting the logging thread.
   /// </summary>
   public Logger(CommandLineArguments? commandLineArguments)
   {
      commandLineArguments?.ApplyTo(this);

      SetOutputFile(LogFilePath);

      m_Thread = new(() =>
      {
         while (!m_IsDisposed)
         {
            lock (s_SyncObject)
            {
               (m_ActiveLogList, m_WriteLogList) = (m_WriteLogList, m_ActiveLogList);
            }

            foreach (Log log in m_WriteLogList)
            {
               LogInternal(log);
            }


            FlushConsoleBuffer();
            m_WriteLogList.Clear();

            Thread.Sleep(150);
         }
      });

      m_Thread.Start();
   }

   /// <summary>
   /// Sets the output file for logging. If a file is already open, it is closed before opening the new file.
   /// </summary>
   /// <param name="outputFilePath">The path to the log file. Pass null to close any existing log file without opening a new one.</param>
   public void SetOutputFile(string? outputFilePath)
   {
      if (m_FileStream != null)
      {
         m_FileOutput?.Flush();
         m_FileOutput?.Dispose();
         m_FileStream?.Dispose();
         m_FileOutput = null;
         m_FileStream = null;
      }

      if (outputFilePath != null)
      {
         try
         {
            m_FileStream = File.Open(outputFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
            m_FileOutput = new BufferedStream(m_FileStream, 16 * 1024);
            WriteLogFileHeader();
         }
         catch { }
      }
   }

   private void WriteLogFileHeader()
   {
      m_StringBuilder.Clear();
      m_StringBuilder.Append("---------------------------------------------------------------\n");
      m_StringBuilder.Append("LOG HEADER\n\n");
      m_StringBuilder.AppendFormat("Date: {0:yyyy/MM/dd HH:mm:ss}\n", DateTime.Now);
      m_StringBuilder.AppendFormat("System: {0}\n", Environment.OSVersion);
      m_StringBuilder.AppendFormat("Command Line: {0}\n", Environment.CommandLine);
      m_StringBuilder.AppendFormat("Working Directory: {0}\n", Environment.CurrentDirectory);
      m_StringBuilder.Append("===============================================================\n");

      byte[] bytes = Encoding.UTF8.GetBytes(m_StringBuilder.ToString());
      m_FileOutput!.Write(bytes);
   }

   /// <summary>
   /// Releases all resources used by the Logger.
   /// </summary>
   void IDisposable.Dispose()
   {
      m_IsDisposed = true;
      m_Thread.Join();

      Flush();

      SetOutputFile(null);
   }

   private void Flush()
   {
      foreach (Log log in m_WriteLogList)
      {
         LogInternal(log);
      }

      foreach (Log log in m_ActiveLogList)
      {
         LogInternal(log);
      }

      FlushConsoleBuffer();
   }

   /// <inheritdoc />
   public void Log
   (
      in LogScope scope,
      LogLevel logLevel,
      [InterpolatedStringHandlerArgument("", nameof(logLevel))] LogInterpolatedStringHandler message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      if (logLevel < LogLevel)
      {
         return;
      }

      Log(scope, logLevel, message.GetFormattedText(), exception, sourceFilePath, sourceFileLine, memberName);
   }

   /// <inheritdoc />
   public void Log
   (
      in LogScope scope,
      LogLevel logLevel,
      string message,
      Exception? exception = null,
      [CallerFilePath] string sourceFilePath = "Unknown",
      [CallerLineNumber] int sourceFileLine = -1,
      [CallerMemberName] string memberName = "Unknown"
   )
   {
      if (logLevel < LogLevel)
      {
         return;
      }

      lock (s_SyncObject)
      {
         m_ActiveLogList.Add(new()
         {
            Message = message,
            LogLevel = logLevel,
            SourceFilePath = sourceFilePath,
            SourceFileLine = sourceFileLine,
            MemberName = memberName,
            TimeStamp = DateTime.Now,
            Exception = exception,
            Label = scope.Name
         });
      }
   }

   private unsafe void LogInternal(in Log log)
   {
      m_StringBuilder.Clear();

      Span<char> lineprefixBuffer = stackalloc char[64];
      Span<char> linePrefix = WriteLinePrefix(log, lineprefixBuffer);

      for (int i = 0; i < log.Message.Length;)
      {
         m_StringBuilder.Append(linePrefix);

         int start = i;
         int length = 0;
         while (i < log.Message.Length)
         {
            bool isCarriageReturn = log.Message[i] == '\r';
            if (isCarriageReturn || log.Message[i] == '\n')
            {
               i++;

               if (isCarriageReturn && i < log.Message.Length && log.Message[i] == '\n')
               {
                  i++;
               }

               break;
            }

            i++;
            length++;
         }

         m_StringBuilder.Append(log.Message, start, length);
         m_StringBuilder.Append('\n');
      }

      int lengthBeforeCallerInfo = m_StringBuilder.Length;
      if (LogCallerInfo != LogCallerInfo.None)
      {
         m_StringBuilder.AppendFormat("\t{0}:{1} [{2}]\n", log.SourceFilePath, log.SourceFileLine, log.MemberName);
      }

      if (m_FileStream != null)
      {
         int fileMessageLenth = (LogCallerInfo & LogCallerInfo.File) != 0 ? m_StringBuilder.Length : lengthBeforeCallerInfo;

         Span<char> message = stackalloc char[fileMessageLenth];
         m_StringBuilder.CopyTo(0, message, fileMessageLenth);

         int byteCount = Encoding.UTF8.GetByteCount(message);
         Span<byte> bytes = stackalloc byte[byteCount];
         Encoding.UTF8.GetBytes(message, bytes);

         m_FileOutput!.Write(bytes);
      }

      if (UseConsoleColors)
      {
         ConsoleColor color = log.LogLevel switch
         {
            LogLevel.Trace => ConsoleColor.Magenta,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => throw new NotImplementedException()
         };

         Console.ForegroundColor = color;
      }

      int consoleMessageLength = (LogCallerInfo & LogCallerInfo.Console) != 0 ? m_StringBuilder.Length : lengthBeforeCallerInfo;
      WriteConsole(m_StringBuilder, consoleMessageLength);

      if (UseConsoleColors)
      {
         Console.ResetColor();
      }
   }

   private void WriteConsole(StringBuilder stringBuilder, int length)
   {
      int position = 0;
      while (position < length)
      {
         int available = s_ConsoleOutputBuffer.Length - m_ConsoleOutputBufferPosition;
         int writeLength = Math.Min(length - position, available);
         stringBuilder.CopyTo(position, s_ConsoleOutputBuffer, m_ConsoleOutputBufferPosition, writeLength);

         m_ConsoleOutputBufferPosition += writeLength;
         position += writeLength;

         if (m_ConsoleOutputBufferPosition == s_ConsoleOutputBuffer.Length)
         {
            FlushConsoleBuffer();
         }
      }
   }

   private void FlushConsoleBuffer()
   {
      if (m_ConsoleOutputBufferPosition > 0)
      {
         Console.Out.Write(s_ConsoleOutputBuffer, 0, m_ConsoleOutputBufferPosition);
         m_ConsoleOutputBufferPosition = 0;
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private unsafe Span<char> WriteLinePrefix(in Log log, Span<char> destination)
   {
      int length = 0;
      if (LogTimestamps)
      {
         log.TimeStamp.TryFormat(destination, out length, "yyyy/MM/dd HH:mm:ss ");
      }

      destination[length++] = '[';
      log.Label.AsSpan().CopyTo(destination[length..]);
      length += log.Label.Length;
      destination[length++] = ']';
      destination[length++] = ' ';
      return destination[..length];
   }
}
