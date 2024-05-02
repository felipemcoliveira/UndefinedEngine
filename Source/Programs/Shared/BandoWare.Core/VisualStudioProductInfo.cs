using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BandoWare.Core;

public partial class VisualStudioVersionJsonConverter : VersionJsonConverter
{
   public override Regex VersionRegex => GetGeneratedRegex();

   [GeneratedRegex(@"(?<Major>\d+)\.(?<Minor>\d+).(?<Build>\d).(?<Patch>\d)", RegexOptions.Compiled)]
   private static partial Regex GetGeneratedRegex();
}

[JsonObject(MemberSerialization.OptIn)]
public class VisualStudioProductInfo
{
   [JsonProperty("installationPath")]
   public string InstallationPath { get; set; } = string.Empty;

   [JsonConverter(typeof(VisualStudioVersionJsonConverter))]
   [JsonProperty("installationVersion")]
   public Version Version { get; set; }

   [JsonProperty("displayName")]
   public string DisplayName { get; set; } = string.Empty;

   [JsonProperty("productPath")]
   public string ProductPath { get; set; } = string.Empty;


}
