using System;
using System.Collections.Generic;
using System.IO;

public static class EnvLoader
{
    public static void Load()
    {
        // Procura .env no diretório atual, no diretório pai e na raiz do projeto
        var candidates = new List<string>();
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            if (dir != null)
            {
                // Diretório atual (bin/Debug/netX.0)
                candidates.Add(Path.Combine(dir.FullName, ".env"));
                
                // Diretório pai (SmartBonsaiAPI)
                if (dir.Parent != null)
                {
                    candidates.Add(Path.Combine(dir.Parent.FullName, ".env"));
                    
                    // Diretório avô (src)
                    if (dir.Parent.Parent != null)
                    {
                        candidates.Add(Path.Combine(dir.Parent.Parent.FullName, ".env"));
                        
                        // Raiz do projeto
                        if (dir.Parent.Parent.Parent != null)
                        {
                            candidates.Add(Path.Combine(dir.Parent.Parent.Parent.FullName, ".env"));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EnvLoader] Erro ao descobrir caminhos: {ex.Message}");
        }

        bool loaded = false;
        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                ApplyFile(path);
                Console.WriteLine($"[EnvLoader] Arquivo .env carregado: {path}");
                loaded = true;
                break;
            }
        }

        if (!loaded)
        {
            Console.WriteLine("[EnvLoader] Arquivo .env não encontrado. As variáveis de ambiente devem estar configuradas no sistema.");
        }
    }

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


