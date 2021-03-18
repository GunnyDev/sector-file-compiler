using Compiler.Event;
using Microsoft.JSInterop;

namespace CompilerGUI.Models
{
  public class ConsoleWriter : IEventObserver
  {
    private readonly IJSRuntime js;

    public ConsoleWriter(IJSRuntime js)
    {
      this.js = js;
    }

    public void NewEvent(ICompilerEvent log)
    {
      Write(log.GetMessage());
    }
    public async void Write(string value)
    {
      try
      {
        await js.InvokeVoidAsync("writeLine", value);
      }
      catch
      {

      }
    }
  }
}
