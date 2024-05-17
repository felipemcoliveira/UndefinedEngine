using System;

namespace BandoWare.Core.FileSystem;

public abstract class FileSystemReference
{
   protected readonly string FullName;

   private FileSystemReference()
   {
      throw new InvalidOperationException("Cannot create an instance of FileSystemReference without a path.");
   }

   protected FileSystemReference(string path)
   {
      FullName = path;
   }
}
