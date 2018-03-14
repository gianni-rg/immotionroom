namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using Interfaces;

    public class HelpersNetworkTools : IHelpersNetworkTools
    {
        public string GetLocalIpAddress(int adapterIndex = 0)
        {
            // Find which is the local IP Address
            // See: https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface.getallnetworkinterfaces.aspx
            // See: http://stackoverflow.com/questions/1069103/how-to-get-the-ip-address-of-the-server-on-which-my-c-sharp-application-is-runni

            var ipList = new List<IPAddress>();

            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            bool connectedToANetwork = false;

            foreach (var adapter in adapters)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;

                // Avoid loopback (127.0.0.1)
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    continue;

                var properties = adapter.GetIPProperties();

                try
                {
                    var ipv4Prop = properties.GetIPv4Properties();

                    // Avoid Automatic Private Network Addresses (169.254.X.X)
                    if (ipv4Prop.IsAutomaticPrivateAddressingActive)
                        continue;
                }
                catch (Exception)
                {
                    // No IPv4 Properties
                    continue;
                }

                // Get IPv4 Addresses only
                foreach (var ipAd in properties.UnicastAddresses)
                {
                    if (ipAd.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipList.Add(ipAd.Address);
                    }
                }

                connectedToANetwork = true;
            }

            if (ipList.Count > 0 && adapterIndex < ipList.Count)
            {
                return ipList[adapterIndex].ToString();
            }

            if (adapters.Length > 0 && !connectedToANetwork)
            {
                // Fallback to localhost if no endpoint is found
                return "127.0.0.1";
            }

            return null;
        }
    }
}