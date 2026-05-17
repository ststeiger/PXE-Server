
namespace PXE_Server
{


    public class TFTPServer 
        : System.IDisposable
    {
        Tftp.Net.TftpServer server;
        private static string? ServerDirectory;
        private bool disposedValue;


        public TFTPServer(System.Net.IPAddress localAddress, int port,string rootDirectory)
        {
            server = new Tftp.Net.TftpServer(localAddress,port);
            ServerDirectory = rootDirectory;

            server.OnReadRequest += Server_OnReadRequest;
            server.OnWriteRequest += Server_OnWriteRequest;
            server.Start();
            System.Diagnostics.Trace.WriteLine($"Start TFTPD on {localAddress.ToString()}:{port.ToString()}");
            System.Diagnostics.Trace.WriteLine($"TFTP root dir: {rootDirectory}");
            System.Diagnostics.Trace.Flush();
        }

        private static void Server_OnWriteRequest(Tftp.Net.ITftpTransfer transfer, System.Net.EndPoint client)
        {
            string file = System.IO.Path.Combine(ServerDirectory!, transfer.Filename);

            if (System.IO.File.Exists(file))
            {
                CancelTransfer(transfer, Tftp.Net.TftpErrorPacket.FileAlreadyExists);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting write request from " + client);
                StartTransfer(transfer, new System.IO.FileStream(file, System.IO.FileMode.CreateNew));
            }
        }

        private  static void Server_OnReadRequest(Tftp.Net.ITftpTransfer transfer, System.Net.EndPoint client)
        {
            var path=Utils.CheckFileInRootDir(ServerDirectory!, transfer.Filename);


            System.IO.FileInfo file = new System.IO.FileInfo(path);
            //Is the file within the server directory?
            if (!file.FullName.StartsWith(ServerDirectory!, System.StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, Tftp.Net.TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, Tftp.Net.TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read));
                //StartTransfer(transfer, new MemoryStream(File.ReadAllBytes(file.FullName))); ;
            }
        }

        private static void StartTransfer(Tftp.Net.ITftpTransfer transfer, System.IO.Stream stream)
        {
            transfer.OnProgress += new Tftp.Net.TftpProgressHandler(transfer_OnProgress);
            transfer.OnError += new Tftp.Net.TftpErrorHandler(transfer_OnError);
            transfer.OnFinished += new Tftp.Net.TftpEventHandler(transfer_OnFinished);
            transfer.Start(stream);
        }

        private static void CancelTransfer(Tftp.Net.ITftpTransfer transfer, Tftp.Net.TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }
        static void transfer_OnError(Tftp.Net.ITftpTransfer transfer, Tftp.Net.TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
        }

        static void transfer_OnFinished(Tftp.Net.ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished");
        }

        static void transfer_OnProgress(Tftp.Net.ITftpTransfer transfer, Tftp.Net.TftpTransferProgress progress)
        {
            OutputTransferStatus(transfer, "Progress " + progress);
        }

        private static void OutputTransferStatus(Tftp.Net.ITftpTransfer transfer, string message)
        {
            System.Diagnostics.Trace.WriteLine("[" + transfer.Filename + "] " + message);
            System.Diagnostics.Trace.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    server.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TFTPServer()
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
