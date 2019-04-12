using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Marksman.Service.Descriptors
{
    internal sealed class AssetDescriptor
    {
        #region Instance Properties
        
        public string Serial { get; set; }

        public string Name { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string ModelNumber { get; set; }

        public int AssetType { get; set; }

        #endregion

        #region Constructors

        private AssetDescriptor()
        {
        }

        #endregion

        #region Class Methods
        
        public static AssetDescriptor Create(string hostName)
        {
            try
            {
                CimSessionOptions sessionOptions = new CimSessionOptions() { };
                sessionOptions.AddDestinationCredentials(new CimCredential(ImpersonatedAuthenticationMechanism.Negotiate));
                CimSession session = CimSession.Create(hostName, sessionOptions);

                CimInstance computerDetails = session.EnumerateInstances(@"root\cimv2", "Win32_ComputerSystem").FirstOrDefault();
                CimInstance productDetails = session.EnumerateInstances(@"root\cimv2", "Win32_ComputerSystemProduct").FirstOrDefault();


                //CimInstance biosDetails = session.EnumerateInstances(@"root\cimv2", "Win32_Bios").FirstOrDefault();
                //IEnumerable<CimInstance> monitorDetails = session.EnumerateInstances(@"root\cimv2", "Win32_DesktopMonitor");
                //IEnumerable<CimInstance> processorDetails = session.EnumerateInstances(@"root\cimv2", "Win32_Processor");
                //IEnumerable<CimInstance> memoryDetails = session.EnumerateInstances(@"root\cimv2", "Win32_MemoryArray");
                //IEnumerable<CimInstance> motherBoardDetails = session.EnumerateInstances(@"root\cimv2", "Win32_BaseBoard");
                //IEnumerable<CimInstance> hddDetails = session.EnumerateInstances(@"root\cimv2", "Win32_DiskDrive");
                //IEnumerable<CimInstance> networkDetails = session.EnumerateInstances(@"root\cimv2", "Win32_NetworkAdapter");


                //foreach (var prop in productDetails.CimInstanceProperties)
                //{
                //    System.Console.WriteLine($"{prop.Name}  : {prop.Value}");
                //}

                //IEnumerable<CimInstance> list = session.EnumerateInstances(@"root\cimv2", "Win32_NetworkAdapter");

                //foreach (var item in list)
                //{
                //    foreach (var prop in item.CimInstanceProperties)
                //    {
                //        System.Console.WriteLine($"{prop.Name}  : {prop.Value}");                    
                //    }
                //    Console.WriteLine();
                //}

                return new AssetDescriptor()
                {
                    Serial = Convert.ToString(productDetails.CimInstanceProperties["IdentifyingNumber"].Value, CultureInfo.InvariantCulture),
                    Name = Convert.ToString(computerDetails.CimInstanceProperties["Name"].Value, CultureInfo.InvariantCulture),
                    Manufacturer = Convert.ToString(computerDetails.CimInstanceProperties["Manufacturer"].Value, CultureInfo.InvariantCulture),
                    Model = String.Join(" ", computerDetails.CimInstanceProperties["Manufacturer"].Value, computerDetails.CimInstanceProperties["Model"].Value),
                    ModelNumber = Convert.ToString(computerDetails.CimInstanceProperties["Model"].Value, CultureInfo.InvariantCulture),
                    AssetType = Convert.ToInt32(computerDetails.CimInstanceProperties["PCSystemType"].Value, CultureInfo.InvariantCulture)
                };              
            }
            catch (Exception)
            {
                return null;
            }            
        }

        #endregion
    }
}
