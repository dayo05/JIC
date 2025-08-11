using JIC;
using JIC.JNI;

namespace Example; 

[JClass("java.lang")]
public class Integer: DynJClass {
    public Integer(int a): base(a) {}
}