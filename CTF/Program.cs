using SaneWeb.Resources;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CTF
{
    class Program
    {
        static void Main(string[] args)
        {
            SaneServer ws = new SaneServer(
                (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "CTF.Resources.ViewStructure.xml")),
                "Database\\SaneDB.db", true,
                "http://+:80/");
            ws.addController(typeof(Controller.ControllerMain));
            ws.run();
            Console.WriteLine("Running!");
            Console.ReadKey();
        }
    }
}
