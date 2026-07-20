import { spawn, execFile } from "child_process";
import * as fs from "fs";
import * as path from "path";
import { promisify } from "util";

const execFileAsync = promisify(execFile);

export type OpenResult =
  | { ok: true; editorName: string }
  | { ok: false; errorMessage: string };

/**
 * Сначала используем уже запущенный Visual Studio (devenv),
 * иначе ищем и запускаем Visual Studio 2026.
 */
export async function tryOpenInVisualStudio(filePath: string): Promise<OpenResult> {
  if (!filePath || !filePath.trim()) {
    return { ok: false, errorMessage: "Файл не выбран." };
  }

  if (!fs.existsSync(filePath)) {
    return { ok: false, errorMessage: `Файл не найден: ${filePath}` };
  }

  const runningPath = await findRunningDevEnvPath();
  if (runningPath) {
    return launch(runningPath, filePath);
  }

  const installedPath = await findVisualStudioDevEnv();
  if (installedPath) {
    return launch(installedPath, filePath);
  }

  return {
    ok: false,
    errorMessage:
      "Visual Studio не найден. Установите Visual Studio 2026 или запустите devenv.exe.",
  };
}

function launch(devenvPath: string, filePath: string): OpenResult {
  try {
    // /edit открывает файл в уже запущенном экземпляре, если он есть.
    const child = spawn(devenvPath, ["/edit", filePath], {
      detached: true,
      stdio: "ignore",
      windowsHide: true,
    });
    child.unref();
    return { ok: true, editorName: "Visual Studio" };
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    return {
      ok: false,
      errorMessage: `Не удалось открыть файл в Visual Studio: ${message}`,
    };
  }
}

async function findRunningDevEnvPath(): Promise<string | undefined> {
  if (process.platform !== "win32") {
    return undefined;
  }

  try {
    const { stdout } = await execFileAsync(
      "powershell.exe",
      [
        "-NoProfile",
        "-Command",
        "Get-Process devenv -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path -Unique",
      ],
      { windowsHide: true, timeout: 5000 }
    );

    for (const line of stdout.split(/\r?\n/)) {
      const candidate = line.trim();
      if (candidate && fs.existsSync(candidate)) {
        return candidate;
      }
    }
  } catch {
    // Процесс не найден или PowerShell недоступен.
  }

  return undefined;
}

async function findVisualStudioDevEnv(): Promise<string | undefined> {
  const fromVsWhere = await findViaVsWhere();
  if (fromVsWhere) {
    return fromVsWhere;
  }

  const candidates = [
    path.join(
      process.env["ProgramFiles"] ?? "C:\\Program Files",
      "Microsoft Visual Studio",
      "18",
      "Insiders",
      "Common7",
      "IDE",
      "devenv.exe"
    ),
    path.join(
      process.env["ProgramFiles"] ?? "C:\\Program Files",
      "Microsoft Visual Studio",
      "18",
      "Community",
      "Common7",
      "IDE",
      "devenv.exe"
    ),
    path.join(
      process.env["ProgramFiles"] ?? "C:\\Program Files",
      "Microsoft Visual Studio",
      "18",
      "Professional",
      "Common7",
      "IDE",
      "devenv.exe"
    ),
    path.join(
      process.env["ProgramFiles"] ?? "C:\\Program Files",
      "Microsoft Visual Studio",
      "18",
      "Enterprise",
      "Common7",
      "IDE",
      "devenv.exe"
    ),
    "C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\Common7\\IDE\\devenv.exe",
    "D:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\Common7\\IDE\\devenv.exe",
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate)) {
      return candidate;
    }
  }

  return undefined;
}

async function findViaVsWhere(): Promise<string | undefined> {
  const vswhere = path.join(
    process.env["ProgramFiles(x86)"] ?? "C:\\Program Files (x86)",
    "Microsoft Visual Studio",
    "Installer",
    "vswhere.exe"
  );

  if (!fs.existsSync(vswhere)) {
    return undefined;
  }

  try {
    // Предпочитаем VS 2026 / product line 18, иначе — последнюю установленную.
    const { stdout: preferred } = await execFileAsync(
      vswhere,
      [
        "-latest",
        "-products",
        "*",
        "-version",
        "[18.0,19.0)",
        "-property",
        "productPath",
      ],
      { windowsHide: true, timeout: 8000 }
    );

    const preferredPath = preferred.trim();
    if (preferredPath && fs.existsSync(preferredPath)) {
      return preferredPath;
    }
  } catch {
    // Нет подходящей версии 18.x — пробуем любую последнюю.
  }

  try {
    const { stdout } = await execFileAsync(
      vswhere,
      ["-latest", "-products", "*", "-property", "productPath"],
      { windowsHide: true, timeout: 8000 }
    );

    const productPath = stdout.trim();
    if (productPath && fs.existsSync(productPath)) {
      return productPath;
    }
  } catch {
    // vswhere не вернул путь.
  }

  return undefined;
}
