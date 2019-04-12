using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Marksman.Service
{
    public static class IPScanner
    {
        #region Class Methods
        
        public static IEnumerable<IPScannerResult>  Scan(string[] subnet)
        {
            ConcurrentBag<IPScannerResult> results = new ConcurrentBag<IPScannerResult>();
            var ipAddresses = subnet.SelectMany(s => Enumerable.Range(1, 255).Select(i => s + "." + i));
            Parallel.ForEach(ipAddresses, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, i =>
            {
                var result = IPScanner.ScanAddress(i);
                if (result != null)
                {
                    results.Add(result);
                }
            });
            return results.ToList();
        }

        public static IPScannerResult ScanAddress(string address)
        {
            IPScannerResult result = new IPScannerResult() { IpAddress = address };

            Ping ping = new Ping();
            PingReply pingReply = ping.Send(address);
            result.Status = pingReply.Status;

            if (pingReply.Status == IPStatus.Success)
            {
                try
                {
                    var host = Dns.GetHostEntry(address);
                    result.HostName = host.HostName;
                }
                catch (Exception ex)
                { // Continue. 
                }
            }
            
            Console.WriteLine($"{result.IpAddress} - {result.HostName} - {result.Status}");
            
            return result;
        }

        #endregion
    }

    public class IPScannerResult
    {
        #region Instance Properties        

        public IPStatus Status { get; set; }

        public string IpAddress { get; set; }

        public string HostName { get; set; }

        #endregion

        #region Constructors

        public IPScannerResult()
        {

        }

        #endregion
    }
}
