using Marksman.Service;
using Marksman.Service.Descriptors;
using SnipeSharp;
using System;
using System.Linq;

namespace Marksman
{
    class Program
    {
        public static void Main()
        {

            var devices = IPScanner.Scan(new string[] { "172.16.26" }).Where(i => i.Status == System.Net.NetworkInformation.IPStatus.Success).ToList();
            foreach (var item in devices)
            {
                //var a = AssetDescriptor.Create(item.HostName);                
                //var t = "";
            }





            //// Single Device.
            //SnipeItApi snipe = new SnipeItApi();
            //snipe.ApiSettings.BaseUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["Snipe:ApiAddress"]);
            //snipe.ApiSettings.ApiToken = System.Configuration.ConfigurationManager.AppSettings["Snipe:ApiToken"];            

            //var device = AssetDescriptor.Create("localhost");

            //try
            //{
            //    snipe.SyncAssetDetails(device);
            //}
            //catch (Exception ex)
            //{
                
            //}            


            Console.ReadLine();

            //var rc = HostFactory.Run(x =>
            //{
            //    x.Service<MarksmanService>(s =>
            //    {
            //        s.ConstructUsing(name => new MarksmanService());
            //        s.WhenStarted(tc => tc.Start());
            //        s.WhenStopped(tc => tc.Stop());
            //    });
            //    x.RunAsLocalSystem();

            //    x.SetDescription("Sample Topshelf Host");
            //    x.SetDisplayName("Stuff");
            //    x.SetServiceName("Stuff");
            //});

            //var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            //Environment.ExitCode = exitCode;
        }

    }
}
