using System;
using System.IO;

namespace BandoWare.Core.FileSystem;

public class DirectoryReference : FileSystemReference, IEquatable<DirectoryReference>, IComparable<DirectoryReference>
{
   public string DirectoryName => Path.GetFileName(FullName);

   public DirectoryReference? ParentDirectory
   {
      get
      {
         string? parent = Path.GetDirectoryName(FullName);
         return parent is null ? null : new DirectoryReference(parent);
      }
   }

   public DirectoryReference(string path)
      : base(RemoveTrailingDirectorySeparator(Path.GetFullPath(path)))
   {

   }

   public static string RemoveTrailingDirectorySeparator(string path)
   {
      if (path.Length == 2 && path[1] == ':')
      {
         return path + Path.DirectorySeparatorChar;
      }

      ReadOnlySpan<char> span = path.AsSpan();
      int length = span.Length;
      while (length > 0 && span[length - 1] is '\\' or '/')
      {
         if (length >= 2 && span[length - 2] == ':')
         {
            break;
         }

         length--;
      }

      return path;
   }

   public int CompareTo(DirectoryReference? other)
   {
      return StringComparer.OrdinalIgnoreCase.Compare(FullName, other?.FullName);
   }

   public override bool Equals(object? obj)
   {
      return obj is DirectoryReference other && Equals(other);
   }

   public bool Equals(DirectoryReference? other)
   {
      return other is not null && StringComparer.OrdinalIgnoreCase.Equals(FullName, other.FullName);
   }

   public override int GetHashCode()
   {
      return StringComparer.OrdinalIgnoreCase.GetHashCode(FullName);
   }

   public override string ToString()
   {
      return FullName;
   }
}
