namespace UndefinedCore;

public enum LogCallerInfo
{
   None = 0,
   File = 1,
   Console = 2,
   Everywhere = File | Console
}
