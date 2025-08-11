using JIC;
using JIC.JNI;

namespace Example; 

[JClass("me.ddayo.jic.example")]
public class Test: DynJClass {
    public Test(params object[] par): base(par) {}
}
