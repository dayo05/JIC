# JIC
This project aims to connect C# and Java with DNNE(with hostfxr api) and JNI.

[Related project(node-java)](https://github.com/joeferner/node-java)

I did this project on 2023 and not activly maintained. But if who want to use C# and Java in same project and dont want to use IPC, this code may help you.

### Base sturcture
The entrypoint is located under Java side(Embedding CLR to C looks more clear for when I start project).
Actual implementation of `JIC.clrMain` is located under `JICNative/library.cpp`. Also `library.cpp` provides various functions to communicate with JVM.
> I just uploaded code under my HDD so there are some unnecessary logging functionality. You can remove it if you want.

The `clrMain` native function will invoke function annotated with `MainEntryAttribute` in any scope(Code exists on `JIC.ManagedEntry.EntryExecution()`).

Licensed under MIT
