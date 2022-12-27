using System.Text;
using DeviceVersion10;
using MediaVersion10;
using MediaVersion20;
using System.ServiceModel.Channels;
using System.ServiceModel;
using StreamType = MediaVersion10.StreamType;
using TransportProtocol = MediaVersion10.TransportProtocol;


namespace RtspFinder
{
    public class OnvifRtspFinder
    {
        private string _url;
        private string _username;
        private string _password;
        private IList<string> _allRtspLinks = new List<string>();


        UriBuilder deviceUri;
        MediaClient mediaV10;
        Media2Client mediaV20;
        MediaProfile[] profilesV20;
        Profile[] profilesV10;


        public OnvifRtspFinder(string username, string password, string url)
        {
            _url = url;
            _username = username;
            _password = password;

        }

        public IList<string> GetRtsp()
        {
            ConnectCam();

            return _allRtspLinks;
        }

        private void ConnectCam()
        {
            bool inError = false;
            deviceUri = new UriBuilder("http:");
            deviceUri.Host = _url;

            Binding binding;
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = System.Net.AuthenticationSchemes.Digest;
            binding = new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8), httpTransport);

            try
            {
                Console.WriteLine($"checking {_url} ....");

                DeviceClient device = new DeviceClient(binding, new EndpointAddress(_url));

                Service[] services = device.GetServicesAsync(false).Result.Service;
                var a = device.GetServiceCapabilitiesAsync().Result;
                //var b = device.GetCapabilitiesAsync(CapabilityCategory.All[0]).Result;
                Service xmedia10 = services.SingleOrDefault(s => s.Namespace.Equals( "http://www.onvif.org/ver10/media/wsdl"));
                Service xmedia20 = services.SingleOrDefault(s => s.Namespace.Equals( "http://www.onvif.org/ver20/media/wsdl"));

                if (xmedia10 != null)
                {
                    mediaV10 = new MediaClient(binding, new EndpointAddress(_url));
                    mediaV10.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
                    mediaV10.ClientCredentials.HttpDigest.ClientCredential.Password = _password;

                    //mediaV10.ClientCredentials.UserName.UserName = _username;
                    //mediaV10.ClientCredentials.UserName.Password = _password;


                    profilesV10 = mediaV10.GetProfilesAsync().Result.Profiles;

                    if (profilesV10 != null)
                        for (int i = 0; i < profilesV10.Length; i++)
                        {
                            var rtsp = GetRtspV10(i);

                            if (!_allRtspLinks.Contains(rtsp))
                            {
                                _allRtspLinks.Add(rtsp);
                                Console.WriteLine(rtsp);
                            }
                        }
                }

                if (xmedia20 != null)
                {
                    mediaV20 = new Media2Client(binding, new EndpointAddress(_url));
                    mediaV20.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
                    mediaV20.ClientCredentials.HttpDigest.ClientCredential.Password = _password;

                    profilesV20 = mediaV20.GetProfilesAsync(null, null).Result.Profiles;

                    if (profilesV20 != null)
                        for (int i = 0; i < profilesV20.Length; i++)
                        {
                            var rtsp = GetRtspV20(i);

                            if (!_allRtspLinks.Contains(rtsp))
                            {
                                _allRtspLinks.Add(rtsp);
                                Console.WriteLine(rtsp);
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnAuthorized. username: {_username}, password: {_password}");
                Console.WriteLine(ex.Message);
                inError = true;
            }
        }

        private string GetRtspV10(int profileIndex)
        {
            var streamSetup = new MediaVersion10.StreamSetup();

            var tran = new MediaVersion10.Transport();
            var tunnelTran = new MediaVersion10.Transport();

            tran.Protocol = TransportProtocol.RTSP;
            tunnelTran.Protocol = TransportProtocol.RTSP;
            tran.Tunnel = tunnelTran;
            tunnelTran.Protocol = TransportProtocol.RTSP;
            streamSetup.Transport = tran;
            streamSetup.Transport.Tunnel = tunnelTran;
            streamSetup.Stream = StreamType.RTPUnicast;

            var uri = mediaV10.GetStreamUriAsync(streamSetup, profilesV10[profileIndex].token).Result;

            var finalUri = uri.Uri.Replace("rtsp://", $"rtsp://{_username}:{_password}@");
            return finalUri;
        }

        private string GetRtspV20(int profileIndex)
        {

            var uri = mediaV20.GetStreamUriAsync
                ("RTSP", profilesV20[profileIndex].token).Result;
            var finalUri = uri.Uri.Replace("rtsp://", $"rtsp://{_username}:{_password}@");
            return finalUri;
        }
    }
}

