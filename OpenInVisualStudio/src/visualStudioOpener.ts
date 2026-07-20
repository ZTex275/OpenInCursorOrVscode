import { spawn, execFile } from "child_process";
import * as fs from "fs";
import * as path from "path";
import { promisify } from "util";

const execFileAsync = promisify(execFile);

const VS_EDITIONS = ["Insiders", "Community", "Professional", "Enterprise"] as const;

export type OpenResult =
  | { ok: true; editorName: string }
  | { ok: false; errorMessage: string };

/**
 * Сначала используем уже запущенный Visual Studio (devenv),
 * иначе ищем установленный: приоритет VS 2026, затем VS 2022.
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
      "Visual Studio не найден. Установите Visual Studio 2026 или 2022.",
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

  for (const candidate of buildKnownDevEnvCandidates()) {
    if (fs.existsSync(candidate)) {
      return candidate;
    }
  }

  return undefined;
}

function buildKnownDevEnvCandidates(): string[] {
  const programFiles = process.env["ProgramFiles"] ?? "C:\\Program Files";
  const roots = [programFiles, "C:\\Program Files", "D:\\Program Files"];
  // 18 = VS 2026, 17 = VS 2022
  const majorVersions = ["18", "17"];
  const candidates: string[] = [];
  const seen = new Set<string>();

  for (const root of roots) {
    for (const major of majorVersions) {
      for (const edition of VS_EDITIONS) {
        const candidate = path.join(
          root,
          "Microsoft Visual Studio",
          major,
          edition,
          "Common7",
          "IDE",
          "devenv.exe"
        );
        const normalized = path.normalize(candidate);
        if (!seen.has(normalized.toLowerCase())) {
          seen.add(normalized.toLowerCase());
          candidates.push(normalized);
        }
      }
    }
  }

  return candidates;
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

  // Сначала VS 2026 (18.x), затем VS 2022 (17.x), затем любая последняя.
  const versionRanges = ["[18.0,19.0)", "[17.0,18.0)"];

  for (const version of versionRanges) {
    try {
      const { stdout } = await execFileAsync(
        vswhere,
        [
          "-latest",
          "-products",
          "*",
          "-version",
          version,
          "-property",
          "productPath",
        ],
        { windowsHide: true, timeout: 8000 }
      );

      const productPath = stdout.trim();
      if (productPath && fs.existsSync(productPath)) {
        return productPath;
      }
    } catch {
      // Нет установки в этом диапазоне версий.
    }
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
