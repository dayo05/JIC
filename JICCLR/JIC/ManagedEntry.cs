using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using JIC.JNI;

namespace JIC;


public static class ManagedEntry {
    private static volatile bool isInitialized = false;
    private static volatile object ol = new();

    private static readonly Dictionary<Assembly, Dictionary<string, Type>> jClassType = new();
    private static readonly Dictionary<string, JClassInfo> jClassInstance = new();

    internal static Type GetOrDefaultType(Assembly asm, string target) {
        if (!jClassType.TryGetValue(asm, out var d)) return typeof(DynJClass);
        return d.TryGetValue(target, out var t) ? t : typeof(DynJClass);
    }
    internal static JClassInfo GetJClass(string package, string cname) {
        var jniSign = package + "." + cname;
        return jClassInstance.TryGetValue(jniSign, out var cls) ? cls
            : jClassInstance[jniSign] = new JClassInfo(package, cname);
    }

    internal static JClassInfo GetJClass(Type t) {
        var attr = t.GetCustomAttribute<JClassAttribute>();
        if (attr == null) throw new TypeLoadException($"Expected Java class type {t.Name} doesn't has JClass attribute");
        return GetJClass(attr.Package, attr.EntryPoint ?? t.Name);
    }

    [UnmanagedCallersOnly]
    public static unsafe void ClrMain(IntPtr getJClassPtr, IntPtr newInstance, IntPtr getMethod, IntPtr* callMethod, IntPtr getMethodSignature) {
        using var _ = new Benchmark.Benchmark("Main",
            x => { Console.WriteLine($"Time spent on main CLR loop: {x.Ticks / 10.0}us"); });
        lock (ol) {
            if (!isInitialized) {
                InitFn(getJClassPtr, newInstance, getMethod, callMethod, getMethodSignature);
                Init();
            }

            isInitialized = true;
        }
        try {
            EntryExecution();
        }
        catch (Exception e) {
            Console.WriteLine(e);
            Environment.Exit(-3);
        }
    }

    private static unsafe void InitFn(IntPtr getJClassPtr, IntPtr newInstance, IntPtr getMethod, IntPtr* callMethod, IntPtr jGetMethodSignature) {
        JClassInfo.Init(getJClassPtr, newInstance);
        JMethod.Init(getMethod, callMethod, jGetMethodSignature);
    }

    private static List<Assembly> asm;

    private static void Init() {
        using var _ = new Benchmark.Benchmark("Primary", x => { Console.WriteLine($"Time spent on assembly loading: {x.Ticks / 10.0}us"); });
        asm = new();
        if (!File.Exists("integrated.txt"))
            File.Create("integrated.txt").Close();
        
        // dotnet/runtime #88427
        var alc = AssemblyLoadContext.All.First(x => x.Assemblies.Any(x => x == typeof(MainEntryAttribute).Assembly));
        using (var sr = new StreamReader("integrated.txt")) {
            foreach (var libs in sr.ReadToEnd().Split("\n").Where(x => !string.IsNullOrWhiteSpace(x))) {
                asm.Add(alc.LoadFromAssemblyPath(libs));
            }
        }

        foreach (var assembly in asm) {
            foreach (var type in assembly.GetTypes()) {
                var jc = type.GetCustomAttribute<JClassAttribute>();
                if (jc == null) continue;
                var cname = jc.EntryPoint ?? type.Name;
                var cFullName = jc.Package + "." + cname;
                if (!jClassType.TryGetValue(assembly, out var c)) {
                    c = new();
                    jClassType[assembly] = c;
                }
                if (c.ContainsKey(cFullName)) {
                    Console.Error.WriteLine("Only one Class can created from one java class by each assembly");
                    Environment.Exit(-2);
                }
                
                c[cFullName] = type;
                GetJClass(jc.Package, cname);
            }
        }
    }

    private static void EntryExecution() {
        var methodCache = new List<MethodInfo>();
        
        foreach (var m in from assembly in asm from type in assembly.GetTypes() from m in type.GetRuntimeMethods() select m) {
            if (m.GetCustomAttribute<MainEntryAttribute>() == null) continue;
            if (!m.IsStatic) {
                Console.Error.WriteLine("MainEntry attribute must used on static method");
                Environment.Exit(-2);
            }

            if (m.GetParameters().Length != 0) {
                Console.Error.WriteLine("MainEntry must not have any parameters");
                Environment.Exit(-2);
            }
            methodCache.Add(m);
        }

        foreach (var x in methodCache) 
            x.Invoke(null, null);
    }
}