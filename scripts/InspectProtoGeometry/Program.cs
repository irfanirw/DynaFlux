using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        // Find repo root by walking up until a 'libs' folder exists
        var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        System.IO.DirectoryInfo libsDir = null;
        while (dir != null && dir.Parent != null)
        {
            var candidate = System.IO.Path.Combine(dir.FullName, "..", "..", "..", "..", "libs");
            if (System.IO.Directory.Exists(candidate)) { libsDir = new System.IO.DirectoryInfo(candidate); break; }
            dir = dir.Parent;
        }
        if (libsDir == null)
        {
            Console.WriteLine("Could not find libs folder");
            return;
        }
        // Ensure dependent assemblies are loaded first
        var dynServices = System.IO.Path.Combine(libsDir.FullName, "DynamoServices.dll");
        var dynCore = System.IO.Path.Combine(libsDir.FullName, "DynamoCore.dll");
        foreach(var dep in new[] { dynServices, dynCore })
        {
            if (System.IO.File.Exists(dep))
            {
                Console.WriteLine("Loading dependency: " + dep);
                Assembly.LoadFrom(dep);
            }
        }

        var asmPath = System.IO.Path.Combine(libsDir.FullName, "ProtoGeometry.dll");
        Console.WriteLine("Trying to load: " + asmPath);
        var asm = Assembly.LoadFrom(asmPath);
        Console.WriteLine("Assembly: " + asm.FullName);
        // List candidate types containing Mesh/Vector/Point substrings
        var all = asm.GetTypes().Select(t=>t.FullName).OrderBy(n=>n).ToList();
        var candidates = all.Where(n => n.Contains("Mesh") || n.Contains("Vector") || n.Contains("Point")).ToList();
        Console.WriteLine("Found types:");
        foreach(var c in candidates) Console.WriteLine(c);

        // Inspect first matches
        foreach(var name in candidates.Where(n=>n.Contains("Mesh")).Take(1))
        {
            var t = asm.GetType(name);
            Console.WriteLine($"\nType: {t.FullName}");
            Console.WriteLine("--- Methods ---");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
            {
                var sig = m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")";
                Console.WriteLine(sig);
            }
            Console.WriteLine("--- Properties ---");
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(p=>p.Name))
            {
                Console.WriteLine(p.PropertyType.Name + " " + p.Name);
            }
        }

        foreach(var name in candidates.Where(n=>n.Contains("Vector")).Take(1))
        {
            var t = asm.GetType(name);
            Console.WriteLine($"\nType: {t.FullName}");
            Console.WriteLine("--- Methods ---");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
            {
                var sig = m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")";
                Console.WriteLine(sig);
            }
            Console.WriteLine("--- Properties ---");
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(p=>p.Name))
            {
                Console.WriteLine(p.PropertyType.Name + " " + p.Name);
            }
        }

        foreach(var name in candidates.Where(n=>n.Contains("Point")).Take(1))
        {
            var t = asm.GetType(name);
            Console.WriteLine($"\nType: {t.FullName}");
            Console.WriteLine("--- Methods ---");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
            {
                var sig = m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")";
                Console.WriteLine(sig);
            }
            Console.WriteLine("--- Properties ---");
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OrderBy(p=>p.Name))
            {
                Console.WriteLine(p.PropertyType.Name + " " + p.Name);
            }
        }
    }
}