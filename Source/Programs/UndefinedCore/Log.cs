using System;

namespace UndefinedCore;

public readonly struct Log
{
   public string Label { get; init; }
   public string Message { get; init; }
   public string SourceFilePath { get; init; }
   public int SourceFileLine { get; init; }
   public string MemberName { get; init; }
   public LogLevel LogLevel { get; init; }
   public DateTime TimeStamp { get; init; }
   public Exception? Exception { get; init; }
}
