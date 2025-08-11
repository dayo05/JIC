cmake_minimum_required(VERSION 3.26)
project(JICNative)

set(CMAKE_CXX_STANDARD 17)

include_directories(/Users/dayo/Library/Java/JavaVirtualMachines/azul-17.0.4.1/Contents/Home/include /Users/dayo/Library/Java/JavaVirtualMachines/azul-17.0.4.1/Contents/Home/include/darwin /Users/dayo/JIC/JICCLR/JIC/bin/Release/net6.0)

link_directories(/Users/dayo/Library/Java/JavaVirtualMachines/azul-17.0.4.1/Contents/Home/lib)
link_libraries(/Users/dayo/JIC/JICCLR/JIC/bin/Release/net6.0/JICNE.dylib)

add_library(JICNative SHARED library.cpp)
