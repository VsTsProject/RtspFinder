using System.Security.Policy;
using System.ServiceModel;
using DeviceVersion10;
using System.ServiceModel.Channels;
using OnvifDiscovery;
using RtspFinder;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;



// find devices with other lang like C++

//var host = Dns.GetHostEntry(Dns.GetHostName());
//foreach (var ip in host.AddressList)
//{
//    if (ip.AddressFamily == AddressFamily.InterNetwork)
//    {
//        Console.WriteLine(ip.ToString());
//    }
//}

var discovery = new Discovery();
var deviceFinder = new DeviceFinder(discovery);
var devices = await deviceFinder.FindDevices();

IDictionary<string, string[]> userInformations = new Dictionary<string, string[]>();

//userInformations.Add("admin", new string[] { "vee22cam", "40799111a" });
//userInformations.Add("ImageProcessing1", new string[] { "Veerasense123." });
//userInformations.Add("admin", new string[] { "vee22cam" });
userInformations.Add("admin", new string[] { "aaaAAA111", "admin", "Rezaei@9441"});
userInformations.Add("root", new string[] { "9441" });

var counter = 0;

//var rtspFinder = new OnvifRtspFinder("admin", "Rezaei@9441", "http://192.168.1.9/onvif/device_service");
//var rtspFinder = new OnvifRtspFinder("admin", "admin", "http://192.168.1.250/onvif/device_service");
//var result = rtspFinder.GetRtsp();

IDictionary<string, IList<string>> mainResult = new Dictionary<string, IList<string>>();

foreach (var device in devices)
{
    counter++;
    Console.WriteLine($"camera number {counter}");

    foreach (KeyValuePair<string, string[]> info in userInformations)
    {
        foreach (var pass in info.Value)
        {
            var rtspFinder = new OnvifRtspFinder(info.Key, pass, device);
            var result = rtspFinder.GetRtsp();

            if (!mainResult.ContainsKey(device) && result.Count != 0)
            {
                mainResult.Add(device, result);
            }

            Console.WriteLine();
        }
    }
}

Console.WriteLine($"{mainResult.Count} devices and it's rtsps were found");

