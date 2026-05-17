
namespace PXE_Server
{


    // https://stackoverflow.com/a/14328085
    public class IPSegment
    {

        private System.UInt32 _ip;
        private System.UInt32 _mask;

        public IPSegment(string ip, string mask)
        {
            _ip = ip.ParseIp();
            _mask = mask.ParseIp();
        }

        public System.UInt32 NumberOfHosts
        {
            get { return ~_mask + 1; }
        }

        public System.UInt32 NetworkAddress
        {
            get { return _ip & _mask; }
        }

        public System.UInt32 BroadcastAddress
        {
            get { return NetworkAddress + ~_mask; }
        }

        public System.Collections.Generic.IEnumerable<System.UInt32> Hosts()
        {
            for (uint host = NetworkAddress + 1; host < BroadcastAddress; host++)
            {
                yield return host;
            }
        }

    }


    public static class IpHelpers
    {
        public static string ToIpString(this System.UInt32 value)
        {
            uint bitmask = 0xff000000;
            string[] parts = new string[4];
            for (int i = 0; i < 4; i++)
            {
                uint masked = (value & bitmask) >> ((3 - i) * 8);
                bitmask >>= 8;
                parts[i] = masked.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return string.Join(".", parts);
        }

        public static System.UInt32 ParseIp(this string ipAddress)
        {
            string[] splitted = ipAddress.Split('.');
            System.UInt32 ip = 0;
            for (int i = 0; i < 4; i++)
            {
                ip = (ip << 8) + System.UInt32.Parse(splitted[i]);
            }
            return ip;
        }

        public static System.Net.IPAddress ToIpAddress(this System.UInt32 value)
        {
            return System.Net.IPAddress.Parse(value.ToIpString());
        }
    }


}
