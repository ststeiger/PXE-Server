
namespace PXE_Server
{

    using Microsoft.AspNetCore.Builder; // for Use*, Map*
    using Microsoft.AspNetCore.Hosting; // for ConfigureKestrel


    public class HttpFileServer 
        : System.IDisposable
    {
        private Microsoft.AspNetCore.Builder.WebApplication? _app;
        private System.Threading.CancellationTokenSource? _cts;

        public int Port { get; }
        public string RootDirectory { get; }

        public HttpFileServer(int port, string rootPath)
        {
            Port = port;
            RootDirectory = rootPath;
            // Ensure directory exists
            if (!System.IO.Directory.Exists(RootDirectory))
                System.IO.Directory.CreateDirectory(RootDirectory);
        }

        public void Start()
        {
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = 
                Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();

            // Configure Kestrel to listen on the specific port
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(Port);
            });

            _app = builder.Build();


            // Configure the HTTP request pipeline.
            if (Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.IsDevelopment(_app.Environment))
            {
                _app.UseExceptionHandler("/Error");
            }


            _app.UseRouting();
            // _app.UseAuthorization();


            // Setup the MIME type provider to handle PXE files (bin, efi, etc.)
            Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider provider = 
                new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();


            provider.Mappings[".*"] = "application/octet-stream"; // Catch-all rule fallback

            // Optional: Map specific PXE extensions if needed, or rely on DefaultContentType
            provider.Mappings[".iso"] = "application/x-iso9660-image";

            _app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions()
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(RootDirectory),
                RequestPath = "", // Serve from root
                ContentTypeProvider = provider,
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });


            _app.MapGet("/", () => "Kestrel Server is running publicly!");

            // _app.MapStaticAssets();
            // _app.MapRazorPages().WithStaticAssets();

            // Start the server in the background
            _cts = new System.Threading.CancellationTokenSource();
            Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(_app, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _app?.DisposeAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}