namespace UndefinedCore;

public struct Range(int start, int end)
{
   public static Range Invalid => new(-1, -1);

   public int Start { get; set; } = start;
   public int End { get; set; } = end;

   public readonly int Length => End - Start;

   public bool IsValid => Start >= 0 && End >= 0 && End >= Start;

   public static implicit operator Range((int Start, int End) range)
   {
      return new(range.Start, range.End);
   }
}
