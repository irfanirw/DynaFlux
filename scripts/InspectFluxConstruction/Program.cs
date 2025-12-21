// See https://aka.ms/new-console-template for more information
using System.Reflection;

var assemblyPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../libs/DynaFluxCore.dll"));
var asm = Assembly.LoadFrom(assemblyPath);

Type[] rawTypes;
try
{
    rawTypes = asm.GetTypes();
}
catch (ReflectionTypeLoadException ex)
{
    rawTypes = ex.Types.Where(t => t != null).ToArray()!;
}

var types = rawTypes
    .Where(t => t.FullName != null && t.FullName.Contains("FluxConstruction", StringComparison.Ordinal))
    .OrderBy(t => t.FullName)
    .ToList();

if (types.Count == 0)
{
    Console.WriteLine("No FluxConstruction types found.");
    return;
}

foreach (var type in types)
{
    Console.WriteLine(type.FullName);
    foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
        Console.WriteLine($"  {prop.Name}: {prop.PropertyType.FullName} (settable: {prop.CanWrite})");
    }
    foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
    {
        var parameters = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.FullName} {p.Name}"));
        Console.WriteLine($"  ctor({parameters})");
    }
}
