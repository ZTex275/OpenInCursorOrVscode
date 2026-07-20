import * as vscode from "vscode";
import { tryOpenInVisualStudio } from "./visualStudioOpener";

export function activate(context: vscode.ExtensionContext): void {
  const disposable = vscode.commands.registerCommand(
    "openInVisualStudio.openFile",
    async (uri?: vscode.Uri) => {
      const filePath = resolveTargetFilePath(uri);
      if (!filePath) {
        void vscode.window.showWarningMessage("Файл не выбран.");
        return;
      }

      const result = await tryOpenInVisualStudio(filePath);
      if (!result.ok) {
        void vscode.window.showWarningMessage(result.errorMessage);
      }
    }
  );

  context.subscriptions.push(disposable);
}

export function deactivate(): void {
  // no-op
}

function resolveTargetFilePath(uri?: vscode.Uri): string | undefined {
  if (uri?.scheme === "file") {
    return uri.fsPath;
  }

  const active = vscode.window.activeTextEditor?.document;
  if (active && !active.isUntitled && active.uri.scheme === "file") {
    return active.uri.fsPath;
  }

  return undefined;
}
