using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompilerGUI.Models
{
  public class CompilerSettings
  {
    public const string DefaultBuildVersion = "BUILD_VERSION";
    public ObservableCollection<ConfigFile> ConfigFiles { get; } = new();
    public bool ValidateOutput { get; set; } = true;

    public bool StripComments { get; set; }

    public string Version { get; set; } = DefaultBuildVersion;
  }
}
