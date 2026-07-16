using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace OpenInCursorOrVscode
{
    internal static class EditorOpener
    {
        public static bool TryOpen(string filePath, out string editorName, out string errorMessage)
        {
            editorName = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "Файл не выбран.";
                return false;
            }

            if (!File.Exists(filePath))
            {
                errorMessage = "Файл не найден: " + filePath;
                return false;
            }

            string cursorPath = FindCursor();
            if (!string.IsNullOrEmpty(cursorPath))
            {
                return Launch(cursorPath, filePath, "Cursor", out editorName, out errorMessage);
            }

            string vscodePath = FindVsCode();
            if (!string.IsNullOrEmpty(vscodePath))
            {
                return Launch(vscodePath, filePath, "Visual Studio Code", out editorName, out errorMessage);
            }

            errorMessage = "Не найдены Cursor и Visual Studio Code. Установите один из редакторов.";
            return false;
        }

        private static bool Launch(string executablePath, string filePath, string name, out string editorName, out string errorMessage)
        {
            editorName = name;
            errorMessage = null;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "\"" + filePath + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Не удалось открыть файл в " + name + ": " + ex.Message;
                return false;
            }
        }

        private static string FindCursor()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            string[] candidates =
            {
                Path.Combine(localAppData, "Programs", "cursor", "Cursor.exe"),
                Path.Combine(localAppData, "cursor", "Cursor.exe"),
                Path.Combine(programFiles, "Cursor", "Cursor.exe"),
                Path.Combine(programFilesX86, "Cursor", "Cursor.exe"),
                Path.Combine(programFiles, "cursor", "Cursor.exe"),
                Path.Combine(programFilesX86, "cursor", "Cursor.exe"),
                @"C:\Program Files\Cursor\Cursor.exe",
                @"D:\Program Files (x86)\cursor\Cursor.exe",
                @"D:\Program Files\Cursor\Cursor.exe"
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            string fromAppPaths = FindFromAppPaths("Cursor.exe");
            if (!string.IsNullOrEmpty(fromAppPaths))
            {
                return fromAppPaths;
            }

            return FindInPath("cursor", "Cursor.exe");
        }

        private static string FindVsCode()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            string[] candidates =
            {
                Path.Combine(localAppData, "Programs", "Microsoft VS Code", "Code.exe"),
                Path.Combine(programFiles, "Microsoft VS Code", "Code.exe"),
                Path.Combine(programFilesX86, "Microsoft VS Code", "Code.exe"),
                @"D:\Program Files\Microsoft VS Code\Code.exe",
                @"D:\Program Files (x86)\Microsoft VS Code\Code.exe"
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            string fromRegistry = FindVsCodeFromRegistry();
            if (!string.IsNullOrEmpty(fromRegistry))
            {
                return fromRegistry;
            }

            string fromAppPaths = FindFromAppPaths("Code.exe");
            if (!string.IsNullOrEmpty(fromAppPaths))
            {
                return fromAppPaths;
            }

            return FindInPath("code", "Code.exe");
        }

        private static string FindVsCodeFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\Code.exe\shell\open\command"))
                {
                    string command = key?.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(command))
                    {
                        string path = Unquote(command.Split(' ')[0]);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string FindFromAppPaths(string exeName)
        {
            string[] roots =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + exeName
            };

            foreach (string root in roots)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(root))
                    {
                        string path = key?.GetValue(null) as string;
                        path = Unquote(path);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            return path;
                        }
                    }

                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(root))
                    {
                        string path = key?.GetValue(null) as string;
                        path = Unquote(path);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static string FindInPath(string commandName, string executableFileName)
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
            {
                return null;
            }

            foreach (string folder in pathVariable.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                string directory = folder.Trim();
                string exeCandidate = Path.Combine(directory, executableFileName);
                if (File.Exists(exeCandidate))
                {
                    return exeCandidate;
                }

                string cmdCandidate = Path.Combine(directory, commandName + ".cmd");
                if (File.Exists(cmdCandidate))
                {
                    string resolved = ResolveCmdToExecutable(cmdCandidate, executableFileName);
                    if (!string.IsNullOrEmpty(resolved))
                    {
                        return resolved;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// code.cmd / cursor.cmd в PATH указывают на GUI .exe рядом с установкой.
        /// Запускаем .exe напрямую, чтобы не мигало консольное окно.
        /// </summary>
        private static string ResolveCmdToExecutable(string cmdPath, string executableFileName)
        {
            string binDir = Path.GetDirectoryName(cmdPath);
            if (string.IsNullOrEmpty(binDir))
            {
                return null;
            }

            // VS Code: ...\Microsoft VS Code\bin\code.cmd -> ...\Code.exe
            string vsCodeStyle = Path.GetFullPath(Path.Combine(binDir, "..", executableFileName));
            if (File.Exists(vsCodeStyle))
            {
                return vsCodeStyle;
            }

            // Cursor: ...\cursor\resources\app\bin\cursor.cmd -> ...\Cursor.exe
            string cursorStyle = Path.GetFullPath(Path.Combine(binDir, "..", "..", "..", executableFileName));
            if (File.Exists(cursorStyle))
            {
                return cursorStyle;
            }

            return null;
        }

        private static string Unquote(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = value.Trim();
            if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}
