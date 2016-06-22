using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FjeeaRefresher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ValidationCodeParser.InitStore(FjeeaResource.ResourceGetter.GetResources());
#if DEBUG
            {
                var pathName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                var exampleConfig = pathName + "/../../Config.example.json";
                try
                {
                    Config.Load();
                } catch (Exception)
                {
                    File.Copy(exampleConfig, pathName + "/Config.json");
                    Config.Load();
                }
            }
#else
            Config.Load();
#endif

        }
    }
}
