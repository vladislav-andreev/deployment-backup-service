using static System.Environment;

namespace DeploymentBackUpService;

class Program
{
    static void Main()
    {
        string userProfile = GetFolderPath(SpecialFolder.UserProfile);
        string sourceFolder = Path.Combine(userProfile, "Source", "Repos");
        string archiveFolder = Path.Combine(userProfile, "OneDrive", "Job", "Archive", "Deployment Files");
        foreach (DirectoryInfo mainFolder in new DirectoryInfo(sourceFolder).GetDirectories())
        {
            DirectoryInfo source = mainFolder.GetDirectories().FirstOrDefault(f => f.Name == "Deployment");
            if (source == null) continue;
            DirectoryInfo target = new(Path.Combine(archiveFolder, mainFolder.Name));
            CopyAll(source, target);
            Console.WriteLine($"Checked and updated {mainFolder.Name}");
        }

        foreach (DirectoryInfo archive in new DirectoryInfo(archiveFolder).GetDirectories())
        {
            DirectoryInfo folder = new(Path.Combine(sourceFolder, archive.Name));
            if (!folder.Exists) continue;
            FileInfo[] sourceFiles = folder.GetFiles("*", SearchOption.AllDirectories);
            int deletedFiles = 0;
            foreach (FileInfo archiveFile in archive.GetFiles("*", SearchOption.AllDirectories))
            {
                if (sourceFiles.Any(f => f.Name == archiveFile.Name)) continue;
                archiveFile.Delete();
                deletedFiles++;
            }

            if (deletedFiles == 0) continue;
            Console.WriteLine($"Deleted {deletedFiles:N0} file(s) from {archive.Name}");
        }

        Console.WriteLine("Press any key");
        Console.Read();

        static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            IEnumerable<FileInfo> files = source.GetFiles();
            foreach (FileInfo file in files.Where(f => f.Directory?.Name != "Deployment" || f.Extension != ".msi"))
            {
                FileInfo targetFile = new(Path.Combine(target.FullName, file.Name));
                if (targetFile.Exists && file.LastWriteTime == targetFile.LastWriteTime &&
                    file.Length == targetFile.Length) continue;
                file.CopyTo(targetFile.FullName, true);
            }

            foreach (DirectoryInfo folder in source.GetDirectories().Where(f => f.Name != "Publish"))
            {
                DirectoryInfo nextFolder = target.CreateSubdirectory(folder.Name);
                CopyAll(folder, nextFolder);
            }
        }
    }
}