#include <cstring>
#include "library.h"

static JNIEnv* env;
static jclass JICMain;
static jmethodID getSignature;
JNIEXPORT void Java_me_ddayo_jic_JIC_clrMain(JNIEnv* env_) {
    env = env_;
    JICMain = env->FindClass("me/ddayo/jic/JIC");
    getSignature = env->GetStaticMethodID(JICMain, "getSignature",
                                          "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");
    auto fns = new intptr_t[10];

    fns[0] = (intptr_t) callJvmMethodI;
    fns[1] = (intptr_t) callJvmMethodJ;
    fns[2] = (intptr_t) callJvmMethodZ;
    fns[3] = (intptr_t) callJvmMethodF;
    fns[4] = (intptr_t) callJvmMethodB;
    fns[5] = (intptr_t) callJvmMethodC;
    fns[6] = (intptr_t) callJvmMethodD;
    fns[7] = (intptr_t) callJvmMethodS;
    fns[8] = (intptr_t) callJvmMethodObj;
    fns[9] = (intptr_t) callJvmMethodV;

    ClrMain((intptr_t) getJvmClass, (intptr_t) newJvmInstance, (intptr_t) getJvmMethod, fns, (intptr_t) getReturnType);
}

int callJvmMethodI(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallIntMethodA((jclass)self, (jmethodID)methodPtr, params);
}
long long callJvmMethodJ(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallLongMethodA((jclass)self, (jmethodID)methodPtr, params);
}
bool callJvmMethodZ(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallBooleanMethodA((jclass)self, (jmethodID)methodPtr, params);
}
float callJvmMethodF(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallFloatMethodA((jclass)self, (jmethodID)methodPtr, params);
}
char callJvmMethodB(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallByteMethodA((jclass)self, (jmethodID)methodPtr, params);
}
unsigned short callJvmMethodC(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallCharMethodA((jclass)self, (jmethodID)methodPtr, params);
}
double callJvmMethodD(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallDoubleMethodA((jclass)self, (jmethodID)methodPtr, params);
}
short callJvmMethodS(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallShortMethodA((jclass)self, (jmethodID)methodPtr, params);
}
void* callJvmMethodObj(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    return env->CallObjectMethodA((jclass)self, (jmethodID)methodPtr, params);
}
void callJvmMethodV(void* self, void* methodPtr, int paramCnt, jvalue* params) {
    auto f = fopen("asdffdsa.asdf1", "wt");
    for(int i = 0; i < paramCnt; i++)
        fprintf(f, "%d %ld %f %lf\n", params[i].i, params[i].j, params[i].f, params[i].d);
    fclose(f);
    env->CallVoidMethodA((jclass)self, (jmethodID)methodPtr, params);
}

void* getReturnType(char* className, char* name, char* signature) {
    auto cn = env->NewStringUTF(className);
    auto n = env->NewStringUTF(name);
    auto s = env->NewStringUTF(signature);

    /*
    auto jc = env->FindClass("me/ddayo/jic/JIC");
    auto gs = env->GetStaticMethodID(JICMain, "getSignature",
                                          "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");

    auto rtn = (jstring)env->CallStaticObjectMethod(jc, gs, cn, n, s);
     */
    auto rtn = (jstring)env->CallStaticObjectMethod(JICMain, getSignature, cn, n, s);

    env->DeleteLocalRef(cn);
    env->DeleteLocalRef(n);
    env->DeleteLocalRef(s);

    int lnt = env->GetStringUTFLength(rtn);
    if(lnt == 0) {
        env->DeleteLocalRef(rtn);
        return nullptr;
    }
    void* str = new char[lnt + 1];
    jboolean b;
    const char* t = env->GetStringUTFChars(rtn, &b);
    memcpy(str, t, lnt);
    //env->ReleaseStringUTFChars(rtn, (const char*)str);
    env->DeleteLocalRef(rtn);
    return str;
}

void* getJvmMethod(void* classInfo, void* className, char* name, char* signature) {
    return env->GetMethodID((jclass)classInfo, name, signature);
}
void* newJvmInstance(void* selfInfo, void* ctorInfo, int paramCnt, jvalue* params) {
    auto f = fopen("asdfasdfasdfasdfasdfasdf.a", "wt");
    fprintf(f, "%lld %lld", (long long)selfInfo, (long long)ctorInfo);
    fclose(f);
    auto instance = env->NewObjectA((jclass) selfInfo, (jmethodID) ctorInfo, params);
    return instance;
}
void* getJvmClass(void* name) {
    printf("Trying to find class: %s\n", (char*)name);
    return env->FindClass((char*)name);
}
