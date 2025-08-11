using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JIC.JNI; 

internal class JClassInfo {
    private delegate IntPtr GetJClassPtr(string cls);

    private static GetJClassPtr getJClassPtr { get; set; }

    private unsafe delegate IntPtr NewInstance(IntPtr selfInfo, IntPtr methodHandle, int paramCnt, JMethod.JniValues* par);
    private static NewInstance newInstance;

    internal string FullName => Package + "/" + Name;
    internal string FullJName => Package + "." + Name;
    
    private static readonly Dictionary<JMethodSignature, JMethod> methods = new(new JMethodSignatureHash());

    private static bool isInitialized = false;
    internal static void Init(IntPtr jClassPtr, IntPtr jNewInstance) {
        getJClassPtr = Marshal.GetDelegateForFunctionPointer<GetJClassPtr>(jClassPtr);
        newInstance = Marshal.GetDelegateForFunctionPointer<NewInstance>(jNewInstance);
        isInitialized = true;
    }
    
    public string Package { get; }
    internal string Name { get; }
    internal readonly string JNIName;
    internal IntPtr NativeClassInfo;
    
    internal JClassInfo(string package, string name) {
        if (!isInitialized) throw new InvalidProgramException("JClass Not initialized yet!");
        Package = package;
        Name = name;
        JNIName = package.Replace('.', '/') + "/" + name;
        NativeClassInfo = getJClassPtr(JNIName);
    }

    internal IntPtr Ctor(params object[] par) {
        var buildSignature = GetSignature(par);
        var methodInfo = new JMethodSignature("<init>", buildSignature.ToArray());
        if (!methods.ContainsKey(methodInfo))
            methods[methodInfo] = new JMethod(this, methodInfo);

        var uni = JMethod.GetJniValues(par);
        unsafe {
            fixed (JMethod.JniValues* _par = uni) {
                var c = newInstance(NativeClassInfo, methods[methodInfo].methodHandle, par.Length, _par);
                return c;
            }
        }
    }

    internal dynamic Invoke(IntPtr self, string method, params object[] par) {
        var buildSignature = GetSignature(par);
        var methodInfo = new JMethodSignature(method, buildSignature.ToArray());
        if (!methods.ContainsKey(methodInfo))
            methods[methodInfo] = new JMethod(this, methodInfo);
        return methods[methodInfo].Invoke(self, par);
    }

    internal static List<string> GetSignature(IEnumerable<dynamic> obj)
        => new(obj.Select(GetSignature));

    private static string GetSignature(dynamic obj) => obj switch {
        int _ => "I",
        bool _ => "Z",
        char _ => "C",
        short _ => "S",
        long _ => "J",
        float _ => "F",
        double _ => "D",
        byte _ => "B",
        DynJClass jc => $"L{jc.jci.JNIName};",
        _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
    };
}


public class DynJClass : DynamicObject {
    internal JClassInfo jci;
    internal IntPtr nativeInstance;
    
    /*
     * Constructor without Constructor call
     */
    internal DynJClass(JClassInfo jc) {
        jci = jc;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected DynJClass(params object[] objs) {
        jci = ManagedEntry.GetJClass(GetType());
        nativeInstance = jci.Ctor(objs);
    }

    private bool isHooked;
    private void Hook() {
        if (isHooked) throw new InvalidOperationException("Cannot hook twice");
        isHooked = true;
    }

    protected dynamic Throw() => throw new NotImplementedException();
    
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object? result) {
        result = jci.Invoke(nativeInstance, binder.Name, args);
        return true;
    }
}