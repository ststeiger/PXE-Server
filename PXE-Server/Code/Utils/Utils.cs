
namespace PXE_Server
{
    class Utils
    {
        public static string CheckFileInRootDir(string rootDir,string? file)
        {
            if(string.IsNullOrEmpty(file))
                file = string.Empty;

            string tmp = file.Replace('/', '\\');
            if (tmp.StartsWith('\\'))
                tmp = tmp.Substring(1);

            string path = System.IO.Path.Combine(rootDir, tmp);
            return path;
        }
    }
}
