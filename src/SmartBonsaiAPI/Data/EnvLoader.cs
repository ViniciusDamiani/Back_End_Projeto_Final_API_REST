using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class EnvLoader
{
    private static string? _loadedFrom = null;

    public static void Load()
    {
        var candidates = new List<string>();
        
        try
        {
            var baseDir = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir))
            {
                candidates.Add(Path.Combine(baseDir, ".env"));
                
                var dir = new DirectoryInfo(baseDir);
                if (dir?.Parent != null)
                {
                    candidates.Add(Path.Combine(dir.Parent.FullName, ".env"));
                    
                    if (dir.Parent.Parent != null)
                    {
                        candidates.Add(Path.Combine(dir.Parent.Parent.FullName, ".env"));
                        
                        if (dir.Parent.Parent.Parent != null)
                        {
                            candidates.Add(Path.Combine(dir.Parent.Parent.Parent.FullName, ".env"));
                        }
                    }
                }
            }

            var currentDir = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(currentDir))
            {
                candidates.Add(Path.Combine(currentDir, ".env"));
            }

            try
            {
                var solutionDir = FindSolutionDirectory();
                if (!string.IsNullOrEmpty(solutionDir))
                {
                    candidates.Add(Path.Combine(solutionDir, ".env"));
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EnvLoader] Erro ao descobrir caminhos: {ex.Message}");
        }

        candidates = candidates.Distinct().Where(p => !string.IsNullOrEmpty(p)).ToList();

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                ApplyFile(path);
                _loadedFrom = path;
                Console.WriteLine($"[EnvLoader] Arquivo .env carregado de: {path}");
                return;
            }
        }

        Console.WriteLine("[EnvLoader] Nenhum arquivo .env encontrado. Procurou em:");
        foreach (var path in candidates)
        {
            Console.WriteLine($"  - {path}");
        }
    }

    private static string? FindSolutionDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (current.GetFiles("*.sln").Length > 0)
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        return null;
    }

    public static string? GetLoadedFrom() => _loadedFrom;

    private static void ApplyFile(string filePath)
    {
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line.Substring(0, idx).Trim();
            var value = line.Substring(idx + 1).Trim();

            // Remove aspas simples/duplas se presentes
            if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            if (!string.IsNullOrEmpty(key))
            {
                // Não sobrescreve se já existir definida no ambiente
                var existing = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(existing))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}
