using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Marksman
{
    class Program
    {
        public static void Main()
        {
            var rc = HostFactory.Run(x =>                                   //1
            {
                x.Service<MarksmanService>(s =>                                   //2
                {
                    s.ConstructUsing(name => new MarksmanService());                //3
                    s.WhenStarted(tc => tc.Start());                         //4
                    s.WhenStopped(tc => tc.Stop());                          //5
                });
                x.RunAsLocalSystem();                                       //6

                x.SetDescription("Sample Topshelf Host");                   //7
                x.SetDisplayName("Stuff");                                  //8
                x.SetServiceName("Stuff");                                  //9
            });                                                             //10

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
            Environment.ExitCode = exitCode;
        }
    }
}
