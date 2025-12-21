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

var type = rawTypes.FirstOrDefault(t => t.FullName == "DynaFluxCore.FluxSurface");
if (type == null)
{
    Console.WriteLine("FluxSurface type not found.");
    foreach (var t in rawTypes.Where(t => t.Name.Contains("FluxSurface", StringComparison.Ordinal)))
    {
        Console.WriteLine($"Found: {t.FullName}");
    }
    return;
}

Console.WriteLine(type.FullName);
foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    try
    {
        Console.WriteLine($"{prop.Name}: {prop.PropertyType.FullName} (settable: {prop.CanWrite})");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"{prop.Name}: <missing dependency> (settable: {prop.CanWrite})");
    }
}

foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
{
    var parameters = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.FullName} {p.Name}"));
    Console.WriteLine($"ctor({parameters})");
}
