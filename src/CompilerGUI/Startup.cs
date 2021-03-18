using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CompilerGUI
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddRazorPages();
      services.AddServerSideBlazor();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });

      if (HybridSupport.IsElectronActive)
      {
        ElectronBootstrap();
      }
    }

    public async void ElectronBootstrap()
    {
      var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
      {
        Width = 1200,
        Height = 750,
        Show = false,
        Resizable = false
      });

      await browserWindow.WebContents.Session.ClearCacheAsync();
      var menu = new MenuItem[] {

                new MenuItem {
                  Label = "Window",
                  Role = MenuRole.window,
                  Type = MenuType.submenu,
                  Submenu = new MenuItem[] {
                     new MenuItem {
                       Label = "Minimize",
                       Accelerator = "CmdOrCtrl+M",
                       Role = MenuRole.minimize
                     },
                     new MenuItem {
                       Label = "Close",
                       Accelerator = "CmdOrCtrl+W",
                       Role = MenuRole.close
                     }
                }
                },
                  new MenuItem {
                    Label = "Help",
                    Role = MenuRole.help,
                    Type = MenuType.submenu,
                    Submenu = new MenuItem[] {
                      new MenuItem
                      {
                          Label = "Learn More",
                          Click = async () => await Electron.Shell.OpenExternalAsync("https://github.com/VATSIM-UK/sector-file-compiler")
                      }
                     }
                    }
                };

      Electron.Menu.SetApplicationMenu(menu);
      browserWindow.OnReadyToShow += () => browserWindow.Show();
      browserWindow.SetTitle("Vatsim UK Sector File Creator");
    }
  }
}
