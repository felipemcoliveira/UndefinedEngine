namespace UndefinedCore;

public static class LogServiceExtensionMethods
{
   public static ScopedLogger CreateScope(this ILogger? service, string scopeName) => CreateScope(service, new LogScope(scopeName));

   public static ScopedLogger CreateScope(this ILogger? service, in LogScope scope)
   {
      return new ScopedLogger
      {
         Scope = scope,
         UnderlyingLogger = service
      };
   }
}
