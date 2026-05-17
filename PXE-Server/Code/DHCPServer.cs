
namespace PXE_Server
{

    using DHCP = GitHub.JPMikkers.DHCP;


    public class DHCPServer 
        : DHCP.DHCPServer
    {
        public System.Net.IPAddress BindAddress { get; set; } = System.Net.IPAddress.Parse("192.168.1.27");
        public Loader? Loader { get; set; } 
        public string? HTTPBootFile { get; set; }

        public DHCPServer(System.Net.IPAddress address) : this(address, 67) { }
        public DHCPServer(System.Net.IPAddress address, int port) : base(null)
        {
            BindAddress = address;

            this.Loader = PXE_Server.Loader.SYSLINUX;

            this.EndPoint = new System.Net.IPEndPoint(BindAddress, port); // default port
            this.SubnetMask = System.Net.IPAddress.Parse("255.255.255.0");
            this.PoolStart = System.Net.IPAddress.Parse("192.168.1.100");
            this.PoolEnd = System.Net.IPAddress.Parse("192.168.1.200");
            this.LeaseTime = DHCP.Utils.InfiniteTimeSpan;
            this.OfferExpirationTime = System.TimeSpan.FromSeconds(30);

            this.MinimumPacketSize = 576;


            this.OnStatusChange += Dhcpd_OnStatusChange;
            this.OnTrace += Dhcpd_OnTrace;

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
                option: new DHCP.DHCPOptionRouter()
                {
                    IPAddresses = new[] { BindAddress }
                }));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
               option: new DHCP.DHCPOptionServerIdentifier(BindAddress)));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
              option: new DHCP.DHCPOptionTFTPServerName(System.Net.Dns.GetHostName())));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
                 option: new DHCP.DHCPOptionHostName(System.Net.Dns.GetHostName())));

        }

        // public readonly record struct BootKey(Loader Loader, byte Architecture);
        public class BootKey
        {
            public readonly Loader Loader;
            public readonly byte Architecture;

            public BootKey(Loader loader, byte architecture)
            {
                this.Loader = loader;
                this.Architecture = architecture;
            }

            // This is required for the Dictionary to compare values instead of memory addresses
            public override bool Equals(object? obj)
            {
                BootKey? other = obj as BootKey;
                if (other == null) 
                    return false;

                return this.Loader == other.Loader && this.Architecture == other.Architecture;
            }

            // This ensures the Dictionary can efficiently bucket the keys
            public override int GetHashCode()
            {
                // Simple hash combining for older .NET versions
                return (int)Loader ^ Architecture;
            }
        }

        // Client architecture codes: https://www.ietf.org/assignments/dhcpv6-parameters/dhcpv6-parameters.xml#processor-architecture
        // 00 = BIOS/Legacy, 06 = x86 UEFI, 07 = x64 UEFI, 11 = ARM64 UEFI
        //
        // iPXE binary variants (from https://boot.ipxe.org/):
        //   ipxe.efi     → iPXE drives the NIC directly (best performance, needs built-in driver)
        //   snponly.efi  → UEFI firmware drives the NIC via SNP (best for modern/ARM64 hardware)
        //   legacy.efi   → UEFI firmware via old UNDI path (best for old firmware)
        // Pick the variant that works best for your hardware and rename it to match the filename below.
        private readonly System.Collections.Generic.Dictionary<BootKey, string> availableArch = 
            new System.Collections.Generic.Dictionary<BootKey, string>()
        {
            { new BootKey(PXE_Server.Loader.SYSLINUX, 0), "lpxelinux.0" },
            { new BootKey(PXE_Server.Loader.SYSLINUX, 6), "syslinux32.efi" },
            { new BootKey(PXE_Server.Loader.SYSLINUX, 7), "syslinux64.efi" },

            { new BootKey(PXE_Server.Loader.IPXE, 0), "ipxe.pxe" },
            { new BootKey(PXE_Server.Loader.IPXE, 7), "ipxe.efi" },
            { new BootKey(PXE_Server.Loader.IPXE, 11), "ipxe.efi" },

            { new BootKey(PXE_Server.Loader.SHIM_GRUB2, 0), "grub2.pxe" },
            { new BootKey(PXE_Server.Loader.SHIM_GRUB2, 7), "shimx64.efi" },

            { new BootKey(PXE_Server.Loader.UEFI_HTTP, 7), "shimx64.efi" }
        };

        protected override void ProcessingReceiveMessage(
            GitHub.JPMikkers.DHCP.DHCPMessage sourceMsg,
            GitHub.JPMikkers.DHCP.DHCPMessage targetMsg
        )
        {
            string? bootFile = string.Empty;

            // Client is a UEFI HTTP Boot capable firmware - no TFTP involved at all.
            // Return a full HTTP URL to the boot script.
            // e.g. "http://192.168.1.21:8080/autoexec.ipxe"
            if (sourceMsg.isHTTP())
            {
                bootFile = HTTPBootFile;
            }
            // Client has already loaded iPXE (via TFTP in the previous step) and is now
            // asking "what script should I run?". iPXE has built-in HTTP support so we
            // return just the filename - iPXE will fetch it via TFTP from our server.
            // e.g. "autoexec.ipxe"
            else if (sourceMsg.isIPXE())
            {
                // this is ipxe script
                // bootFile = "http://192.168.1.27:8080/boot.ipxe";
                bootFile = HTTPBootFile;
            }
            // Client is a raw PXE firmware that only speaks TFTP.
            // Return the appropriate bootloader binary for its architecture,
            // which will then chainload into iPXE. e.g. "ipxe.efi" for ARM64/x64 UEFI.
            else if (sourceMsg.isPXE())
            {
                byte arch = sourceMsg.GetArch();
                // bootFile = avalibleArch[(Loader, arch)];
                // We create a temporary key to perform the lookup
                BootKey lookupKey = new BootKey(this.Loader!.Value, arch); // Replace Loader.IPXE with your specific logic if needed

                // if (availableArch.ContainsKey(lookupKey))
                bootFile = availableArch[lookupKey];
            }


            targetMsg.BootFileName = bootFile;
            targetMsg.NextServerIPAddress = BindAddress;
        }

        public new void Start()
        {
            base.Start();
        }


        private void Dhcpd_OnTrace(object? sender, DHCP.DHCPTraceEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(e?.Message);
            System.Diagnostics.Trace.Flush();
        }

        private void Dhcpd_OnStatusChange(object? sender, DHCP.DHCPStopEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(e?.Reason);
            System.Diagnostics.Trace.Flush();
        }
    }


    public static class DHCPMessageExtensions
    {


        public static byte GetArch(this GitHub.JPMikkers.DHCP.DHCPMessage message)
        {
            try
            {
                if (message.Options != null)
                {
                    foreach (DHCP.IDHCPOption option in message.Options)
                    {
                        // Check if the option type matches
                        if (option.OptionType == GitHub.JPMikkers.DHCP.TDHCPOption.ClientSystemArchitectureType)
                        {
                            // Cast to the generic type to access the Data property
                            DHCP.DHCPOptionGeneric? genericOption = option as GitHub.JPMikkers.DHCP.DHCPOptionGeneric;

                            // Check for null and ensure Data has at least two bytes (to safely access index 1)
                            if (genericOption != null && genericOption.Data != null && genericOption.Data.Length > 1)
                            {
                                return genericOption.Data[1];
                            }
                        }
                    }
                }
            }
            catch
            {
                // Maintain original behavior of returning 0 on any failure
            }

            return 0;
        }

        public static string GetVendorClass(this GitHub.JPMikkers.DHCP.DHCPMessage message)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Iterate directly through the options collection
            foreach (DHCP.IDHCPOption option in message.Options)
            {
                // Check if the option matches the specific type we need
                if (option.OptionType == GitHub.JPMikkers.DHCP.TDHCPOption.VendorClassIdentifier)
                {
                    // Cast to the specific option class to access the Data property
                    DHCP.DHCPOptionVendorClassIdentifier? vendorOption = option as GitHub.JPMikkers.DHCP.DHCPOptionVendorClassIdentifier;

                    if (vendorOption != null && vendorOption.Data != null)
                    {
                        string s = System.Text.Encoding.ASCII.GetString(vendorOption.Data);
                        sb.AppendLine(s);
                    }
                }
            }

            return sb.ToString();
        }


        public static bool isHTTP(this GitHub.JPMikkers.DHCP.DHCPMessage message)
        {
            return GetVendorClass(message).Contains("HTTPClient");
        }

        public static bool isPXE(this GitHub.JPMikkers.DHCP.DHCPMessage message)
        {
            return GetVendorClass(message).Contains("PXEClient");
        }


        public static bool isIPXE(this GitHub.JPMikkers.DHCP.DHCPMessage message)
        {
            // Option 77 is the User Class Identifier
            const GitHub.JPMikkers.DHCP.TDHCPOption UserClassOption = (GitHub.JPMikkers.DHCP.TDHCPOption)77;

            if (message.Options == null) return false;

            foreach (DHCP.IDHCPOption option in message.Options)
            {
                // 1. Check if the option type matches
                if (option.OptionType == UserClassOption)
                {
                    // 2. Safely cast to the generic option type to access the Data byte array
                    DHCP.DHCPOptionGeneric? genericOption = option as GitHub.JPMikkers.DHCP.DHCPOptionGeneric;

                    if (genericOption != null && genericOption.Data != null)
                    {
                        // 3. Convert bytes to string and perform the comparison
                        string userClass = System.Text.Encoding.ASCII.GetString(genericOption.Data);

                        if (string.Equals(userClass, "iPXE", System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}
