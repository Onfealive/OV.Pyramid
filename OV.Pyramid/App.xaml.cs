using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
           
            if (e.Args != null && e.Args.Count() > 0)
            {
                this.Properties["ArbitraryArgName"] = e.Args[0];
            }

            /*
            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                string fname = "No filename given";
                try
                {
                    fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];
                    // It comes in as a URI; this helps to convert it to a path.
                    Uri uri = new Uri(fname);
                    fname = uri.LocalPath;
                    this.Properties["ArbitraryArgName"] = fname;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    // For some reason, this couldn't be read as a URI.

                    // Do what you must...

                }
            } */
            base.OnStartup(e);
        }
    }
}
