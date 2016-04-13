using RobloxSharp;
using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CTF.Controller
{
    public static class ControllerMain
    {
        [Controller("~/a/")]
        public static String a(HttpListenerContext context, String body, String id)
        {
            String actual = Encoding.ASCII.GetString(Convert.FromBase64String(id));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.roblox.com/Login/Negotiate.ashx?suggest=" + actual);
            CookieContainer container = new CookieContainer();
            request.Headers.Add("RBXAuthenticationNegotiation", @": http://www.roblox.com");
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,*");
            request.CookieContainer = container;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return RobloxUtils.buildCookieString(container, response);
        }
    }
}
