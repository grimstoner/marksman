using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Marksman.Service.Descriptors
{
    internal sealed class ComponentDescriptor
    {
        #region Instance Properties

        public string Name { get; set; }

        public string Serial { get; set; }        

        public int Category { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        #endregion

        #region Constructors

        private ComponentDescriptor()
        {

        }

        #endregion

        #region Class Methods

        public static IEnumerable<ComponentDescriptor> Create(string hostName)
        {
            // Validate parameters.
            if (String.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException("hostName");
            }

            try
            {
                CimSessionOptions sessionOptions = new CimSessionOptions() { };
                sessionOptions.AddDestinationCredentials(new CimCredential(ImpersonatedAuthenticationMechanism.Negotiate));
                CimSession session = CimSession.Create(hostName, sessionOptions);

                //IEnumerable<CimInstance> list = session.EnumerateInstances(@"root\cimv2", "Win32_Processor");
                //foreach (var item in list)
                //{
                //    foreach (var prop in item.CimInstanceProperties)
                //    {
                //        System.Console.WriteLine($"{prop.Name}  : {prop.Value}");
                //    }
                //    Console.WriteLine();
                //}
              
                return
                    ComponentDescriptor.GetProcessors(session)
                    .Concat(ComponentDescriptor.GetMemory(session))
                    .Concat(ComponentDescriptor.GetHardDrives(session))
                    .ToList();                     

                //CimInstance computerDetails = session.EnumerateInstances(@"root\cimv2", "Win32_ComputerSystem").FirstOrDefault();
                //CimInstance productDetails = session.EnumerateInstances(@"root\cimv2", "Win32_ComputerSystemProduct").FirstOrDefault();


                //CimInstance biosDetails = session.EnumerateInstances(@"root\cimv2", "Win32_Bios").FirstOrDefault();
                //IEnumerable<CimInstance> monitorDetails = session.EnumerateInstances(@"root\cimv2", "Win32_DesktopMonitor");
                //IEnumerable<CimInstance> processorDetails = session.EnumerateInstances(@"root\cimv2", "Win32_Processor");
                //IEnumerable<CimInstance> memoryDetails = session.EnumerateInstances(@"root\cimv2", "Win32_MemoryArray");
                //IEnumerable<CimInstance> motherBoardDetails = session.EnumerateInstances(@"root\cimv2", "Win32_BaseBoard");
                //
                //IEnumerable<CimInstance> networkDetails = session.EnumerateInstances(@"root\cimv2", "Win32_NetworkAdapter");


                //foreach (var prop in productDetails.CimInstanceProperties)
                //{
                //    System.Console.WriteLine($"{prop.Name}  : {prop.Value}");
                //}

               

                //Component c = new Component();
                //c.Category
                //c.Company
                //c.CreatedAt
                //c.Id
                //c.Location
                //c.MinAmt
                //c.Name
                //c.OrderNumber
                //c.PurchaseCost
                //c.PurchaseDate
                //c.Quantity
                //c.Remaining
                //c.SerialNumber
                //c.UpdatedAt
                //c.UserCanCheckout


                
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static IEnumerable<ComponentDescriptor> GetHardDrives(CimSession session)
        {
            // Validate parameters.
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }


            IEnumerable<CimInstance> hddDetails = session.EnumerateInstances(@"root\cimv2", "Win32_DiskDrive");

            var results = 
                hddDetails
                .Where(item => Convert.ToString(item.CimInstanceProperties["Name"].Value, CultureInfo.InvariantCulture).Contains("PHYSICAL"))
                .Select(item => new ComponentDescriptor()
                {
                    Serial = Convert.ToString(item.CimInstanceProperties["SerialNumber"].Value, CultureInfo.InvariantCulture),
                    Name = Convert.ToString(item.CimInstanceProperties["Model"].Value, CultureInfo.InvariantCulture)                   
                });

            return results.ToList();
        }

        private static IEnumerable<ComponentDescriptor> GetMemory(CimSession session)
        {
            // Validate parameters.
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            IEnumerable<CimInstance> memoryDetails = session.EnumerateInstances(@"root\cimv2", "Win32_PhysicalMemory");
            
            var results =
                memoryDetails
                .Select(item => new ComponentDescriptor()
                {
                    Serial = Convert.ToString(item.CimInstanceProperties["SerialNumber"].Value, CultureInfo.InvariantCulture),
                    Name =                        
                        String.Join(" ",
                         Convert.ToString(item.CimInstanceProperties["Manufacturer"].Value, CultureInfo.InvariantCulture).Trim(),
                         Convert.ToString(item.CimInstanceProperties["PartNumber"].Value, CultureInfo.InvariantCulture).Trim(),
                         Convert.ToString(item.CimInstanceProperties["ConfiguredClockSpeed"].Value, CultureInfo.InvariantCulture).Trim(),
                         (((Convert.ToInt64(item.CimInstanceProperties["Capacity"].Value) / 1024f) / 1024f) / 1024f) + "Gb"),
                    Manufacturer = Convert.ToString(item.CimInstanceProperties["Manufacturer"].Value, CultureInfo.InvariantCulture),
                    Model = Convert.ToString(item.CimInstanceProperties["PartNumber"].Value, CultureInfo.InvariantCulture).Trim()
                });            

            return results.ToList();
        }

        private static IEnumerable<ComponentDescriptor> GetProcessors(CimSession session)
        {
            // Validate parameters.
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            IEnumerable<CimInstance> processorDetails = session.EnumerateInstances(@"root\cimv2", "Win32_Processor");

            var results =
               processorDetails
                .Where(item => Convert.ToString(item.CimInstanceProperties["ProcessorType"].Value, CultureInfo.InvariantCulture).Contains("3"))
               .Select(item => new ComponentDescriptor()
               {
                   Serial = Convert.ToString(item.CimInstanceProperties["SerialNumber"].Value, CultureInfo.InvariantCulture),
                   Name = Convert.ToString(item.CimInstanceProperties["Name"].Value, CultureInfo.InvariantCulture),
                   Manufacturer = Convert.ToString(item.CimInstanceProperties["Manufacturer"].Value, CultureInfo.InvariantCulture),

               });

            return results.ToList();
        }

        #endregion
    }
}
