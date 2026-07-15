using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OpenInCursorOrVscode
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Открыть в Cursor или VS Code", "Открывает текущий файл в Cursor или Visual Studio Code", "1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(OpenInCursorOrVscodePackage.PackageGuidString)]
    public sealed class OpenInCursorOrVscodePackage : AsyncPackage
    {
        public const string PackageGuidString = "A7C3E1F4-9B2D-4F6A-8E5C-1D0B3A7F9E42";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await OpenInEditorCommand.InitializeAsync(this);
        }
    }
}
