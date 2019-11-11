using System;
using System.IO;
using System.Linq;

namespace NuLink.Cli
{
    public class UnlinkCommand : INuLinkCommand
    {
        private readonly IUserInterface _ui;

        public UnlinkCommand(IUserInterface ui)
        {
            _ui = ui;
        }

        public void Execute(NuLinkCommandOptions options)
        {
            _ui.ReportMedium(() =>
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader(_ui);
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);

            if (options.Mode == NuLinkCommandOptions.LinkMode.AllToAll)
            {
                foreach (var package in allPackages)
                {
                    UnlinkPackage(package);
                }

                return;
            }

            var requestedPackage = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);

            if (requestedPackage == null)
            {
                _ui.ReportError(() => $"Error: Package not referenced: {options.PackageId}");
                return;
            }

            UnlinkPackage(requestedPackage);
        }

        private void UnlinkPackage(PackageReferenceInfo requestedPackage)
        {
            var status = requestedPackage.CheckStatus();

            if (!status.LibFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
                return;
            }

            if (!status.IsLibFolderLinked)
            {
                _ui.ReportError(() => $"Error: Package {requestedPackage.PackageId} is not linked.");
                return;
            }

            if (!status.LibBackupFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: backup folder not found, expected {requestedPackage.LibBackupFolderPath}");
                return;
            }

            Directory.Delete(requestedPackage.LibFolderPath);
            Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);

            _ui.ReportSuccess(() => $"Unlinked {requestedPackage.PackageId}");
            _ui.ReportSuccess(() => $" {"-X->"} {status.LibFolderLinkTargetPath}", ConsoleColor.Red, ConsoleColor.DarkYellow);
        }
    }
}