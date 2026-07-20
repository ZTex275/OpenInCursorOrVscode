# Open in Cursor / VS Code ↔ Visual Studio

Two companion extensions that open the current file between **Visual Studio** and **Cursor / Visual Studio Code**.

| Project | Runs in | Opens in |
|---------|---------|----------|
| [OpenInCursorOrVscode](#1-openincursororvscode--visual-studio-extension) | Visual Studio 2022 / 2026 | Cursor (preferred) or VS Code |
| [OpenInVisualStudio](#2-openinvisualstudio--cursor--vs-code-extension) | Cursor / VS Code | Visual Studio 2026 (preferred) or 2022 |

---

## English

### Overview

Right-click a file in one editor and open it in the other. Cursor always has priority over VS Code; a running Visual Studio instance is preferred over starting a new one.

### 1. OpenInCursorOrVscode — Visual Studio extension

Adds a context-menu command in Visual Studio:

**Open in Cursor or Visual Studio Code**

Available from:

- Code editor
- XAML text editor
- Solution Explorer (file items)

#### Open logic

1. If **Cursor** is already running → open the file there  
2. Else if **VS Code** is already running → open the file there  
3. Else launch **Cursor** if installed  
4. Else launch **VS Code** if installed  

Launches the real `.exe` (not `.cmd`) so no console window flashes.

#### Requirements

- Visual Studio 2022 or 2026 (Community / Professional / Enterprise), amd64  
- .NET Framework 4.7.2+ (build)  
- Cursor and/or Visual Studio Code installed

#### Install

1. Build Release (see [Build](#build))  
2. Install `OpenInCursorOrVscode\bin\Release\OpenInCursorOrVscode.vsix`  
   (double-click or **Extensions → Manage Extensions → Install from VSIX**)

#### Debug / F5

In this repo (Cursor/VS Code), use **Run and Debug**:

- `OpenInCursorOrVscode → Visual Studio Exp (Release)`  
- or `Both (Release)`

---

### 2. OpenInVisualStudio — Cursor / VS Code extension

Adds a context-menu command:

**Open in Visual Studio**

Available from:

- Explorer (files)
- Editor context menu
- Editor tab context menu

#### Open logic

1. If **Visual Studio** (`devenv`) is already running → open the file there (`/edit`)  
2. Else prefer **Visual Studio 2026** (17→18 product line via vswhere)  
3. Else fall back to **Visual Studio 2022**

#### Requirements

- Cursor or Visual Studio Code  
- Visual Studio 2022 and/or 2026 installed  
- Node.js 18+ (to build)

#### Install

1. Build (see [Build](#build))  
2. Install the generated VSIX:

```bash
cursor --install-extension OpenInVisualStudio/open-in-visual-studio-*.vsix
# or
code --install-extension OpenInVisualStudio/open-in-visual-studio-*.vsix
```

#### Debug / F5

- `OpenInVisualStudio (Release)`  
- or `Both (Release)`

---

### Build

From the repository root:

```bat
build.cmd
build-vs.cmd
build-vscode.cmd
```

PowerShell:

```powershell
.\scripts\Build-All.ps1 -Configuration Release
.\scripts\Build-OpenInCursorOrVscode.ps1 -Configuration Release -Deploy
.\scripts\Build-OpenInVisualStudio.ps1 -SkipInstall
```

Outputs:

- `OpenInCursorOrVscode\bin\Release\OpenInCursorOrVscode.vsix`  
- `OpenInVisualStudio\open-in-visual-studio-*.vsix`

---

### License

This project is licensed under the [MIT License](LICENSE).

---

## Русский

### Обзор

Два связанных расширения: из одного редактора открывают текущий файл в другом. **Cursor** всегда приоритетнее VS Code; уже запущенный Visual Studio предпочтительнее нового запуска.

### 1. OpenInCursorOrVscode — расширение Visual Studio

Пункт контекстного меню:

**Открыть в Cursor или Visual Studio Code**

Доступен в:

- редакторе кода  
- текстовом редакторе XAML  
- Solution Explorer (файлы)

#### Логика открытия

1. Если запущен **Cursor** → открыть в нём  
2. Иначе если запущен **VS Code** → открыть в нём  
3. Иначе запустить **Cursor**, если установлен  
4. Иначе запустить **VS Code**, если установлен  

Запускается `.exe`, а не `.cmd`, без мигания консоли.

#### Требования

- Visual Studio 2022 или 2026 (Community / Professional / Enterprise), amd64  
- .NET Framework 4.7.2+ (для сборки)  
- Установленный Cursor и/или Visual Studio Code

#### Установка

1. Соберите Release (см. [Сборка](#сборка))  
2. Установите `OpenInCursorOrVscode\bin\Release\OpenInCursorOrVscode.vsix`  
   (двойной щелчок или **Расширения → Управление расширениями → Установить из VSIX**)

#### Отладка / F5

В этом репозитории (Cursor/VS Code) в **Run and Debug**:

- `OpenInCursorOrVscode → Visual Studio Exp (Release)`  
- или `Both (Release)`

---

### 2. OpenInVisualStudio — расширение Cursor / VS Code

Пункт контекстного меню:

**Открыть в Visual Studio**

Доступен в:

- Explorer (файлы)  
- контекстном меню редактора  
- контекстном меню вкладки

#### Логика открытия

1. Если запущен **Visual Studio** (`devenv`) → открыть файл там (`/edit`)  
2. Иначе предпочесть **Visual Studio 2026**  
3. Иначе использовать **Visual Studio 2022**

#### Требования

- Cursor или Visual Studio Code  
- Visual Studio 2022 и/или 2026  
- Node.js 18+ (для сборки)

#### Установка

1. Соберите проект (см. [Сборка](#сборка))  
2. Установите VSIX:

```bash
cursor --install-extension OpenInVisualStudio/open-in-visual-studio-*.vsix
# или
code --install-extension OpenInVisualStudio/open-in-visual-studio-*.vsix
```

#### Отладка / F5

- `OpenInVisualStudio (Release)`  
- или `Both (Release)`

---

### Сборка

Из корня репозитория:

```bat
build.cmd
build-vs.cmd
build-vscode.cmd
```

PowerShell:

```powershell
.\scripts\Build-All.ps1 -Configuration Release
.\scripts\Build-OpenInCursorOrVscode.ps1 -Configuration Release -Deploy
.\scripts\Build-OpenInVisualStudio.ps1 -SkipInstall
```

Артефакты:

- `OpenInCursorOrVscode\bin\Release\OpenInCursorOrVscode.vsix`  
- `OpenInVisualStudio\open-in-visual-studio-*.vsix`

---

### Лицензия

Проект распространяется по [лицензии MIT](LICENSE).
