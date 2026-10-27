using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CodexPresence;

public static class DesktopShortcut
{
    public const string IconName = "codex-discord-rich-presence-3.ico";
    public static string Create(string root, string? directory = null)
    {
        var folder = directory ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "Codex Discord Rich Presence.lnk");
        var type = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("Windows shortcut support is unavailable.");
        dynamic shell = Activator.CreateInstance(type)!;
        dynamic link = shell.CreateShortcut(path);
        try
        {
            var target = Path.Combine(root, "Codex Presence.exe");
            if (File.Exists(path) && !string.Equals((string)link.TargetPath, target, StringComparison.OrdinalIgnoreCase))
                throw new IOException("A different Codex Presence shortcut already exists on this desktop.");
            link.TargetPath = target;
            link.WorkingDirectory = root;
            link.Arguments = "";
            link.Description = "Open Codex Discord Rich Presence";
            link.IconLocation = Path.Combine(root, "Assets", IconName) + ",0";
            link.Save();
            return path;
        }
        finally { Marshal.FinalReleaseComObject(link); Marshal.FinalReleaseComObject(shell); }
    }
}
