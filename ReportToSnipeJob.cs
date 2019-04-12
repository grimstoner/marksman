using Quartz;
using SnipeSharp;
using SnipeSharp.Endpoints.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marksman
{
    public class ReportToSnipeJob : IJob
    {

        Task IJob.Execute(IJobExecutionContext context)
        {
            Trace.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": Started application.");

            System.Collections.Specialized.NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;


            SnipeItApi snipe = new SnipeItApi();
            snipe.ApiSettings.ApiToken = appSettings["API"];
            snipe.ApiSettings.BaseUrl = new Uri(appSettings["BaseURI"]);

            Sentry mySentry = new Sentry(appSettings); // creating new Sentry (we can have multiple for parallel execution at a later point)

            // Adding what we want
            mySentry.AddQuery("WMI", "SELECT Name, Manufacturer, Model, PCSystemType FROM Win32_ComputerSystem");
            mySentry.AddQuery("WMI", "SELECT IdentifyingNumber FROM Win32_ComputerSystemProduct");
            mySentry.AddQuery("WMI", "SELECT Name FROM Win32_BIOS");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Name,MACAddress FROM Win32_NetworkAdapter WHERE NetEnabled=true AND AdapterTypeId=0 AND netConnectionStatus=2");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Model,SerialNumber FROM Win32_DiskDrive");
            mySentry.AddQuery("WMI", "SELECT EndingAddress FROM Win32_MemoryArray");
            mySentry.AddQuery("WMI", "SELECT Name FROM Win32_DesktopMonitor");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Product,SerialNumber FROM Win32_BaseBoard");
            mySentry.AddQuery("WMI", "SELECT Name,NumberOfCores,NumberOfLogicalProcessors FROM Win32_Processor");

            bool getOU = false;
            bool getOUSuccess = Boolean.TryParse(appSettings["OUEnabled"], out getOU);
            if (getOUSuccess && getOU)
            {
                mySentry.AddQuery("Location", "OU");
            }
            else
            {
                mySentry.AddQuery("Location", "Config");
            }

            mySentry.Run();

            Asset currentAsset = mySentry.GetAsset(appSettings, snipe);
            Model currentModel = mySentry.GetModel(appSettings, snipe);
            Manufacturer currentManufacturer = mySentry.GetManufacturer(appSettings, snipe);
            Category currentCategory = mySentry.GetCategory(appSettings, snipe);
            Company currentCompany = mySentry.GetCompany(appSettings, snipe);
            StatusLabel currentStatusLabel = mySentry.GetStatusLabel(appSettings, snipe);
            Location currentLocation = mySentry.GetLocation(appSettings, snipe);

            //Broker.syncAsset(snipe, currentComputer);
            Broker snipeBroker = new Broker();
            bool connectionStatus = snipeBroker.CheckConnection(appSettings);

            if (connectionStatus)
            {
                snipeBroker.SyncAll(snipe, currentAsset, currentModel, currentManufacturer, currentCategory,
                                    currentCompany, currentStatusLabel, currentLocation);
            }
            else
            {
                Console.WriteLine("ERROR: Could not connect to SnipeIT database instance.");
                // Until a standardized logging framework is set up, quick way to make user see crash message.
                Console.ReadKey();
            }

            Trace.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": Exiting application.");
            Trace.WriteLine(" ");

            return null;
        }
    }
}
