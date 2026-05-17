
namespace PXE_Server
{


    public class HttpFileServer_Simple
        : System.IDisposable
    {
        private System.Net.HttpListener? listener;
        private bool disposedValue;

        private int Port { get; set; } = 80;
        private string RootDirectory { get; set; }


        public HttpFileServer_Simple(int port) : this(port, System.IO.Path.Combine(System.Environment.CurrentDirectory,"wwwroot")) { }
        public HttpFileServer_Simple(int port,string rootPath)
        {
            Port = port;
            RootDirectory = rootPath;
        }

        public void Start()
        {
            Stop();

            listener = new System.Net.HttpListener();
            string prefix = "http://+:" + Port.ToString() + "/";
            listener.Prefixes.Add(prefix);
            //listener.Prefixes.Add("http://*:8181/");
            listener.Start();
            System.Diagnostics.Trace.WriteLine("Start HTTPD on "+prefix);
            System.Diagnostics.Trace.Flush();
            _ = DoLoop();


        }


        async System.Threading.Tasks.Task DoLoop()
        {
            if (listener != null)
            {
                while (listener.IsListening)
                {
                    System.Net.HttpListenerContext ctx = await listener.GetContextAsync();
                    _ = ProcessingRequest(ctx);
                }
            }
        }

        async System.Threading.Tasks.Task ProcessingRequest(System.Net.HttpListenerContext ctx)
        {
            string? filename = ctx.Request?.Url?.AbsolutePath;
            System.Diagnostics.Trace.WriteLine("HTTP request file: "+filename);
            System.Diagnostics.Trace.Flush();

            filename=Utils.CheckFileInRootDir(RootDirectory, filename);

            System.IO.FileInfo info = new System.IO.FileInfo(filename);

            if (info.Exists)
            {
                try
                {
                    ctx.Response.ContentType = "application/octet-stream";
                    ctx.Response.ContentLength64 = info.Length;
                    ctx.Response.AddHeader("Date", System.DateTime.Now.ToString("r"));
                    ctx.Response.AddHeader("Last-Modified", info.LastWriteTime.ToString("r"));

                    using (System.IO.FileStream f = info.OpenRead())
                    {
                        await f.CopyToAsync(ctx.Response.OutputStream);
                    }
                    ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                    await ctx.Response.OutputStream.FlushAsync();
                }
                catch (System.Exception)
                {
                    ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            }
            ctx.Response.OutputStream.Close();
        }

        
        public void Stop()
        {
            listener?.Stop();
            listener?.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HttpFileServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
