using System;
using System.IO;

namespace BandoWare.Core.FileSystem;

public class FileReference : FileSystemReference, IEquatable<FileReference>, IComparable<FileReference>
{
   protected readonly static StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
   protected readonly static StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

   public string Extension => Path.GetExtension(FullName);
   public string FileName => Path.GetFileName(FullName);
   public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullName);

   public FileReference(string path) : base(Path.GetFullPath(path))
   {
      if (FullName[^1] is '\\' or '/')
      {
         throw new ArgumentException("File path cannot end with a directory separator.", nameof(path));
      }
   }

   public int CompareTo(FileReference? other)
   {
      return Comparer.Compare(FullName, other?.FullName);
   }

   public override bool Equals(object? obj)
   {
      return obj is FileReference other && Equals(other);
   }

   public bool Equals(FileReference? other)
   {
      return other is not null && Comparer.Equals(FullName, other.FullName);
   }

   public override int GetHashCode()
   {
      return Comparer.GetHashCode(FullName);
   }

   public override string ToString()
   {
      return FullName;
   }
}
