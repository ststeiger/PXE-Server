
namespace PXE_Server
{
    public class PXEConfig
    {
        public string BindAddress { get; set; } = "192.168.1.21";
        public string NetMask { get; set; } = "255.255.255.0";

        public bool Verbose { get; set; } = true;

        public int DHCPPort { get; set; } = 67;
        public int HTTPPort { get; set; } = 8080;
        public int TFTPPort { get; set; } = 69;

        public string ServerDirectory { get; set; } = "E:\\Program Files\\tftpd64_portable_v4.74\\boot_root"; // System.IO.Path.Combine(Environment.CurrentDirectory, "wwwroot");
        public string Loader { get; set; } = "IPXE"; // IPXE SYSLINUX

        public string? HTTPBootFile { get; set; } 


        const string cfg_file_name = "pxe.conf";
        static System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };


        public static PXEConfig? Load()
        {
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(cfg_file_name);
                return System.Text.Json.JsonSerializer.Deserialize<PXEConfig>(bytes, jsonSerializerOptions);

            }
            catch { return new PXEConfig(); }
        }

        public void Save()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize<PXEConfig>(this, jsonSerializerOptions);
                System.IO.File.WriteAllText(cfg_file_name, json);
            }
            catch { }
        }


    }

    public enum Loader
    {
        SYSLINUX,
        IPXE,
        SHIM_GRUB2,
        UEFI_HTTP
    }

}
