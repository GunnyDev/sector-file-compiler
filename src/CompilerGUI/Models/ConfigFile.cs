namespace CompilerGUI.Models
{
  public class ConfigFile
  {
    public ConfigFile(string path, string filename)
    {
      Path = path;
      FileName = filename;
    }
    public string Path { get; set; }
    public string FileName { get; set; }
  }
}
