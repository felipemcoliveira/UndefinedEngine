using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BandoWare.Core;

public abstract class VersionJsonConverter : JsonConverter
{
   public override bool CanWrite => false;

   public abstract Regex VersionRegex { get; }

   public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
   {
      throw new NotSupportedException("CustomCreationConverter should only be used while deserializing.");
   }

   public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
   {
      if (reader.TokenType == JsonToken.Null)
      {
         return null;
      }

      if (reader.TokenType != JsonToken.String)
      {
         throw new JsonSerializationException("Expected string.");
      }

      string? version = (string?)reader.Value;
      if (version == null)
      {
         throw new JsonSerializationException("Invalid version format.");
      }

      if (!Version.TryExtract(VersionRegex, version, out Version semanticVersion))
      {
         throw new JsonSerializationException("Invalid version format.");
      }

      return semanticVersion;
   }

   public override bool CanConvert(Type objectType)
   {
      return typeof(Version).IsAssignableFrom(objectType);
   }
}

public readonly struct Version : IEquatable<Version>
{
   public int Major { get; }
   public int Minor { get; }
   public int Patch { get; }

   public Version(int major, int minor, int patch)
   {
      Major = major;
      Minor = minor;
      Patch = patch;
   }

   public static bool TryExtract(Regex regex, string input, out Version version)
   {
      Match match = regex.Match(input);
      if (!match.Success)
      {
         version = default;
         return false;
      }

      if (!match.Groups.TryGetValue("Major", out Group? majorGroup) || !majorGroup.Success
         || !match.Groups.TryGetValue("Minor", out Group? minorGroup) || !minorGroup.Success)
      {
         version = default;
         return false;
      }

      bool hasPatch = match.Groups.TryGetValue("Patch", out Group? patchGroup) && patchGroup.Success;

      int major = int.Parse(majorGroup.Value);
      int minor = int.Parse(minorGroup.Value);
      int patch = hasPatch ? int.Parse(patchGroup!.Value) : 0;
      version = new Version(major, minor, patch);
      return true;
   }

   /// <summary>
   /// Parses a semantic version string in the format "Major.Minor.Patch" into a <see cref="Version"/>.
   /// </summary>
   /// <param name="version">The version string to parse.</param>
   /// <param name="semanticVersion">The parsed version.</param>
   /// <returns><c>true</c> if the version string was successfully parsed; otherwise, <c>false</c>.</returns>
   public static bool TryParseSemanticVersionSimple(string version, out Version semanticVersion)
   {
      semanticVersion = default;
      int indexOfFirstDot = version.IndexOf('.');
      if (indexOfFirstDot == -1)
      {
         return false;
      }

      ReadOnlySpan<char> majorStr = version.AsSpan(0, indexOfFirstDot);
      if (!int.TryParse(majorStr, NumberStyles.None, null, out int major))
      {
         return false;
      }

      int indexOfSecondDot = version.IndexOf('.', indexOfFirstDot + 1);
      if (indexOfSecondDot == -1)
      {
         return false;
      }

      ReadOnlySpan<char> minorStr = version.AsSpan(indexOfFirstDot + 1, indexOfSecondDot - indexOfFirstDot - 1);
      if (!int.TryParse(minorStr, NumberStyles.None, null, out int minor))
      {
         return false;
      }

      ReadOnlySpan<char> patchStr = version.AsSpan(indexOfSecondDot + 1);
      if (!int.TryParse(patchStr, NumberStyles.None, null, out int patch))
      {
         return false;
      }

      semanticVersion = new Version(major, minor, patch);
      return true;
   }

   public override string ToString()
   {
      return $"{Major}.{Minor}.{Patch}";
   }

   public override bool Equals(object? obj)
   {
      return obj is Version version && Equals(version);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Major, Minor, Patch);
   }

   public bool Equals(Version other)
   {
      return Major == other.Major &&
             Minor == other.Minor &&
             Patch == other.Patch;
   }

   public static bool operator ==(Version left, Version right)
   {
      return left.Equals(right);
   }

   public static bool operator !=(Version left, Version right)
   {
      return !left.Equals(right);
   }

   public static bool operator <(Version left, Version right)
   {
      if (left.Major < right.Major)
      {
         return true;
      }

      if (left.Major > right.Major)
      {
         return false;
      }

      if (left.Minor < right.Minor)
      {
         return true;
      }

      if (left.Minor > right.Minor)
      {
         return false;
      }

      return left.Patch < right.Patch;
   }

   public static bool operator >(Version left, Version right)
   {
      if (left.Major > right.Major)
      {
         return true;
      }

      if (left.Major < right.Major)
      {
         return false;
      }

      if (left.Minor > right.Minor)
      {
         return true;
      }

      if (left.Minor < right.Minor)
      {
         return false;
      }

      return left.Patch > right.Patch;
   }

   public static bool operator <=(Version left, Version right)
   {
      return left == right || left < right;
   }

   public static bool operator >=(Version left, Version right)
   {
      return left == right || left > right;
   }
}
