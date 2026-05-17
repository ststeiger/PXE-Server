
namespace PXE_Server
{


    internal class NamespaceUpdater
    {

        internal static void UpdateProjectNamespace()
        {
            string targetDirectory = @"D:\Repositories\Projects\PXE-Server\PXE-Server\Pages";
            string oldValue = "PxeWebServer";
            string newValue = "PXE_Server";

            try
            {
                // Check if directory exists to avoid exceptions
                if (!System.IO.Directory.Exists(targetDirectory))
                {
                    System.Console.WriteLine("Directory not found.");
                    return;
                }

                // Get all files in the directory
                string[] files = System.IO.Directory.GetFiles(targetDirectory);

                foreach (string filePath in files)
                {
                    // Read the content of the file
                    string content = System.IO.File.ReadAllText(filePath);

                    // Check if the file actually contains the string to avoid unnecessary writes
                    if (content.Contains(oldValue))
                    {
                        // Replace the text
                        string updatedContent = content.Replace(oldValue, newValue);

                        // Overwrite the file with the new content
                        System.IO.File.WriteAllText(filePath, updatedContent);

                        System.Console.WriteLine($"Updated: {System.IO.Path.GetFileName(filePath)}");
                    }
                }

                System.Console.WriteLine("Process complete.");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
