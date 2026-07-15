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
                    UseShellExecute = false
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
            string[] candidates =
            {
                Path.Combine(localAppData, "Programs", "cursor", "Cursor.exe"),
                Path.Combine(localAppData, "cursor", "Cursor.exe"),
                @"C:\Program Files\Cursor\Cursor.exe"
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return FindInPath("cursor");
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
                Path.Combine(programFiles, "Microsoft VS Code", "bin", "code.cmd")
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

            return FindInPath("code");
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
                        return Unquote(command.Split(' ')[0]);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string FindInPath(string commandName)
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
            {
                return null;
            }

            string[] extensions = { ".exe", ".cmd", ".bat", "" };

            foreach (string folder in pathVariable.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                foreach (string extension in extensions)
                {
                    string candidate = Path.Combine(folder.Trim(), commandName + extension);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
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
