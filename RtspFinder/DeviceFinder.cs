using System.Net.NetworkInformation;
using DeviceVersion10;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text;
using OnvifDiscovery;
using OnvifDiscovery.Models;

namespace RtspFinder
{
    public class DeviceFinder
    {
        private readonly Discovery _discovery;
        private IEnumerable<DiscoveryDevice> _WsDevices;
        private readonly IList<string> _devices = new List<string>();

        public DeviceFinder(Discovery discovery)
        {
            _discovery = discovery;
        }

        public async Task<IList<string>> FindDevices()
        {
            await WsDiscoveryMethod();
            //PingDiscoveryMethod();

            return _devices;
        }

        private void PingDiscoveryMethod()
        {
            Binding binding;
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = System.Net.AuthenticationSchemes.Digest;
            binding = new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8), httpTransport);

            for (var i = 0; i <= 255; i++)
            {
                var ip = $"192.168.1.{i}";
                Console.WriteLine(ip);
                var xAddress = $"http://{ip}/onvif/device_service";

                if (!_devices.Contains(xAddress) && PingHost(ip))
                {
                    DeviceClient device = new DeviceClient(binding, new EndpointAddress(xAddress));

                    try
                    {
                        if (device.Endpoint.Contract.Namespace.Contains("onvif"))
                        {
                            _devices.Add(xAddress);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.HResult);
                        Console.WriteLine(e.Data);
                        Console.WriteLine(e.Data);
                    }
                }
            }
        }

        private async Task WsDiscoveryMethod()
        {
            _WsDevices = await _discovery.Discover(1);

            foreach (var wsDevice in _WsDevices)
            {
                var xAddress = $"http://{wsDevice.Address}/onvif/device_service";
                _devices.Add(xAddress);
            }
        }

        private bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
    }
}
