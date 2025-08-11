#ifndef JICNATIVE_LIBRARY_H
#define JICNATIVE_LIBRARY_H


extern "C" {

#include <jni.h>
#include <JICNE.h>
JNIEXPORT void Java_me_ddayo_jic_CLR_clrMain(JNIEnv *);
void *getJvmMethod(void* classInfo, void *className, char *name, char *signature);
void *newJvmInstance(void *selfInfo, void *ctorInfo, int paramCnt, jvalue* params);
void *getJvmClass(void *name);
void* getReturnType(char* className, char* name, char* signature);

int            callJvmMethodI    (void* self, void* methodPtr, int paramCnt, jvalue* params);
long long      callJvmMethodJ    (void* self, void* methodPtr, int paramCnt, jvalue* params);
bool           callJvmMethodZ    (void* self, void* methodPtr, int paramCnt, jvalue* params);
float          callJvmMethodF    (void* self, void* methodPtr, int paramCnt, jvalue* params);
char           callJvmMethodB    (void* self, void* methodPtr, int paramCnt, jvalue* params);
unsigned short callJvmMethodC    (void* self, void* methodPtr, int paramCnt, jvalue* params);
double         callJvmMethodD    (void* self, void* methodPtr, int paramCnt, jvalue* params);
short          callJvmMethodS    (void* self, void* methodPtr, int paramCnt, jvalue* params);
void*          callJvmMethodObj  (void* self, void* methodPtr, int paramCnt, jvalue* params);
void           callJvmMethodV    (void* self, void* methodPtr, int paramCnt, jvalue* params);

JNIEXPORT void Java_me_ddayo_jic_JIC_clrMain(JNIEnv* env_);
}

#endif //JICNATIVE_LIBRARY_H
