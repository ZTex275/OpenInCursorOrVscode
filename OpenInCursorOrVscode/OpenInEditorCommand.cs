using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace OpenInCursorOrVscode
{
    internal sealed class OpenInEditorCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("B8D4F2A5-0C3E-4A7B-9F6D-2E1C4B8A0F53");

        private readonly AsyncPackage package;

        private OpenInEditorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                _ = new OpenInEditorCommand(package, commandService);
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string filePath = GetTargetFilePath();
            if (!EditorOpener.TryOpen(filePath, out _, out string errorMessage))
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    errorMessage,
                    LocalizedStrings.CommandTitle,
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private string GetTargetFilePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            if (dte == null)
            {
                return null;
            }

            string selectedPath = GetSelectedItemPath(dte);
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                return selectedPath;
            }

            Document activeDocument = dte.ActiveDocument;
            if (activeDocument == null || string.IsNullOrWhiteSpace(activeDocument.FullName))
            {
                return null;
            }

            return activeDocument.FullName;
        }

        private static string GetSelectedItemPath(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SelectedItems selectedItems = dte.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return null;
            }

            foreach (SelectedItem selectedItem in selectedItems)
            {
                ProjectItem projectItem = selectedItem?.ProjectItem;
                if (projectItem == null)
                {
                    continue;
                }

                try
                {
                    string path = projectItem.FileNames[1];
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        return path;
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }
    }
}
