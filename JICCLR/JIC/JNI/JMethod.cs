using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JIC.JNI; 

public class JMethod {
    
    private unsafe delegate void* CallMethodObj(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodObj callMethodObj;
    private unsafe delegate int CallMethodInt(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodInt callMethodInt;
    private unsafe delegate short CallMethodShort(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodShort callMethodShort;
    private unsafe delegate long CallMethodLong(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodLong callMethodLong;
    private unsafe delegate char CallMethodChar(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodChar callMethodChar;
    private unsafe delegate bool CallMethodBoolean(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodBoolean callMethodBoolean;
    private unsafe delegate byte CallMethodByte(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodByte callMethodByte;
    private unsafe delegate float CallMethodFloat(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodFloat callMethodFloat;
    private unsafe delegate double CallMethodDouble(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethodDouble callMethodDouble;
    private unsafe delegate void CallMethod(IntPtr self, IntPtr methodHandle, int paramCnt, JniValues* par);
    private static CallMethod callMethod;

    private unsafe delegate byte* GetMethodSignature(string className, string name, string signature);
    private static GetMethodSignature getMethodSignature;

    private delegate IntPtr GetMethod(IntPtr selfClassInfo, string className, string name, string signature);
    private static GetMethod getMethod;

    internal IntPtr methodHandle;

    internal static unsafe void Init(IntPtr jGetMethod, IntPtr* jCallMethod, IntPtr jGetMethodSignature) {
        getMethod = Marshal.GetDelegateForFunctionPointer<GetMethod>(jGetMethod);
        
        callMethodInt = Marshal.GetDelegateForFunctionPointer<CallMethodInt>(*(jCallMethod + 0));
        callMethodLong = Marshal.GetDelegateForFunctionPointer<CallMethodLong>(*(jCallMethod + 1));
        callMethodBoolean = Marshal.GetDelegateForFunctionPointer<CallMethodBoolean>(*(jCallMethod + 2));
        callMethodFloat = Marshal.GetDelegateForFunctionPointer<CallMethodFloat>(*(jCallMethod + 3));
        callMethodByte = Marshal.GetDelegateForFunctionPointer<CallMethodByte>(*(jCallMethod + 4));
        callMethodChar = Marshal.GetDelegateForFunctionPointer<CallMethodChar>(*(jCallMethod + 5));
        callMethodDouble = Marshal.GetDelegateForFunctionPointer<CallMethodDouble>(*(jCallMethod + 6));
        callMethodShort = Marshal.GetDelegateForFunctionPointer<CallMethodShort>(*(jCallMethod + 7));
        callMethodObj = Marshal.GetDelegateForFunctionPointer<CallMethodObj>(*(jCallMethod + 8));
        callMethod = Marshal.GetDelegateForFunctionPointer<CallMethod>(*(jCallMethod + 9));

        getMethodSignature = Marshal.GetDelegateForFunctionPointer<GetMethodSignature>(jGetMethodSignature);
    }

    private string signature;
    
    internal JMethod(JClassInfo jci, JMethodSignature jmsi) {
        unsafe {
            signature = Marshal.PtrToStringAnsi((IntPtr)getMethodSignature(jci.FullJName, jmsi.Name,
                string.Join("", jmsi.Params))).Replace(".", "/");
        }

        methodHandle = getMethod(jci.NativeClassInfo, jci.Package.Replace('.', '/') + "/" + jci.Name, jmsi.Name, signature);
    }

    internal dynamic Invoke(IntPtr self, params object[] par) {
        var stack = new StackTrace();
        var targetAsm = stack.GetFrames().First(x => {
            var xx = x.GetMethod()?.DeclaringType?.Assembly;
            return xx != GetType().Assembly && xx != null;
        } ).GetMethod().DeclaringType.Assembly;
        
        unsafe {
            fixed (JniValues* union = GetJniValues(par)) {
                dynamic result = null;
                switch (signature[^1]) {
                    case ';':
                        var c = callMethodObj(self, methodHandle, par.Length, union);
                        var cname = signature.Split(')')[1][1..^1];
                        var type = ManagedEntry.GetOrDefaultType(targetAsm, cname);
                        var dc = (DynJClass)RuntimeHelpers.GetUninitializedObject(type);
                        
                        dc.jci = ManagedEntry.GetJClass(string.Join('.', cname.Split('/')[..^1]), cname.Split('/')[^1]);
                        dc.nativeInstance = (IntPtr)c;
                        result = dc;
                        break;
                    case 'B':
                        result = callMethodByte(self, methodHandle, par.Length, union);
                        break;
                    case 'C':
                        result = callMethodChar(self, methodHandle, par.Length, union);
                        break;
                    case 'Z':
                        result = callMethodBoolean(self, methodHandle, par.Length, union);
                        break;
                    case 'F':
                        result = callMethodFloat(self, methodHandle, par.Length, union);
                        break;
                    case 'D':
                        result = callMethodDouble(self, methodHandle, par.Length, union);
                        break;
                    case 'V':
                        callMethod(self, methodHandle, par.Length, union);
                        break;
                    case 'I':
                        result = callMethodInt(self, methodHandle, par.Length, union);
                        break;
                    case 'J':
                        result = callMethodLong(self, methodHandle, par.Length, union);
                        break;
                    case 'S':
                        result = callMethodShort(self, methodHandle, par.Length, union);
                        break;
                }

                return result;
            }
        }
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct JniValues {
        [FieldOffset(0)] public int Int;
        [FieldOffset(0)] public double Double;
        [FieldOffset(0)] public char Char;
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public IntPtr native;
        [FieldOffset(0)] public byte Byte;
        [FieldOffset(0)] public short Short;
        [FieldOffset(0)] public long Long;
        [FieldOffset(0)] public bool Bool;
    }

    public static JniValues[] GetJniValues(object[] par) {
        var _union = new JniValues[par.Length];
        for (var i = 0; i < par.Length; i++)
            _union[i] = par[i] switch {
                DynJClass d => new JniValues { native = d.nativeInstance },
                int d => new JniValues { Int = d },
                short d => new JniValues { Short = d },
                char d => new JniValues { Char = d },
                long d => new JniValues { Long = d },
                float d => new JniValues { Float = d },
                double d => new JniValues { Double = d },
                bool d => new JniValues { Bool = d },
                byte d => new JniValues { Byte = d },
                _ => new JniValues { Long = 0 }
            };
        return _union;
    }
}

public class JMethodSignatureHash : IEqualityComparer<JMethodSignature> {
    public bool Equals(JMethodSignature? x, JMethodSignature? y) {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        if(x.Name != y.Name) return false;
        if (x.Params.Count != y.Params.Count) return false;
        return !x.Params.Where((t, i) => t != y.Params[i]).Any();
    }

    public int GetHashCode(JMethodSignature obj) {
        return HashCode.Combine(obj.Name);
    }
}

public record JMethodSignature: IComparable {
    public readonly string Name;
    public readonly List<string> Params = new();

    public JMethodSignature(string name, params string[] p) {
        Name = name;
        Params.AddRange(p);
    }


    public int CompareTo(object? obj) {
        if (obj is not JMethodSignature signature)
            throw new InvalidCastException($"Cannot compare {obj?.GetType().Name} with JMethodSignature");
        var a = string.Compare(Name, signature.Name, StringComparison.Ordinal);
        if (a != 0) return a;
        using var i1 = Params.GetEnumerator();
        using var i2 = signature.Params.GetEnumerator();
        while (i1.MoveNext()) {
            if (!i2.MoveNext()) return 1;
            a = string.Compare(i1.Current, i2.Current, StringComparison.Ordinal);
            if (a != 0) return a;
        }

        return !i2.MoveNext() ? 0 : -1;
    }
}