# JIC - Java Interop for C#

A high-performance bridge between Java and .NET (C#) using JNI and DNNE, enabling seamless interoperability between JVM and CLR environments.

## Overview

JIC (Java Interop for C#) provides a native bridge that allows Java and C# code to interact directly without IPC (Inter-Process Communication). This project embeds the CLR into the JVM process, enabling dynamic method invocation across language boundaries with minimal overhead.

## Architecture

```
┌─────────────┐     JNI      ┌──────────────┐     DNNE     ┌─────────────┐
│   Java/JVM  │ ◄──────────► │ Native Layer │ ◄──────────► │   C#/.NET   │
│   (Entry)   │              │    (C++)     │              │    (CLR)    │
└─────────────┘              └──────────────┘              └─────────────┘
```

### Key Components

- **JICJVM**: Java-side implementation with JVM entry point
- **JICNative**: Native C++ bridge layer using JNI
- **JICCLR**: .NET-side implementation with CLR hosting
  - **JIC**: Core interop library with JNI bindings
  - **Example**: Sample implementation showing usage patterns
  - **Benchmark**: Performance measurement utilities

## Features

- **Bidirectional Communication**: Call Java methods from C# and vice versa
- **Dynamic Method Resolution**: Automatic method signature detection and binding
- **Type Marshalling**: Automatic conversion between Java and .NET types
- **Performance Optimized**: Direct native calls without serialization overhead
- **Benchmarking Support**: Built-in performance measurement tools

## Requirements

- Java Development Kit (JDK) 17 or higher
- .NET 6.0 SDK or higher
- C++ compiler with C++17 support
- CMake 3.26 or higher (for native library)
- Operating System: macOS, Linux, or Windows

## Installation

### 1. Build the Native Bridge

```bash
cd JICNative
mkdir build && cd build
cmake ..
make
```

### 2. Build the .NET Components

```bash
cd JICCLR
dotnet build -c Release
```

### 3. Build the Java Components

```bash
cd JICJVM
./gradlew build
```

### 4. Configure Integration

Update `JICJVM/integrated.txt` with the path to your compiled .NET assemblies:
```
/path/to/your/JICCLR/Example/bin/Release/net6.0/Example.dll
```

## Usage

### Java Side - Define Entry Point

```java
public class JIC {
    private static native void clrMain();
    
    public static void main(String[] args) {
        System.load("/path/to/libJICNative.dylib");
        clrMain();
    }
}
```

### C# Side - Create Interop Classes

```csharp
using JIC;
using JIC.JNI;

[JClass("com.example.package")]
public class MyJavaClass : DynJClass {
    public MyJavaClass(params object[] args) : base(args) { }
}

public class Program {
    [MainEntry]
    public static void Main() {
        // This method will be called from Java
        dynamic javaObject = new MyJavaClass();
        javaObject.someJavaMethod(123);
        
        var result = javaObject.getResult();
        Console.WriteLine($"Result from Java: {result}");
    }
}
```

### Example Integration

```csharp
// Define a wrapper for Java's Integer class
[JClass("java.lang")]
public class Integer : DynJClass {
    public Integer(int value) : base(value) { }
}

// Use it in your code
[MainEntry]
public static void Example() {
    dynamic integer = new Integer(42);
    double value = integer.doubleValue();
    Console.WriteLine($"Integer as double: {value}");
}
```

## Type Mapping

| Java Type | C# Type | JNI Signature |
|-----------|---------|---------------|
| int       | int     | I             |
| long      | long    | J             |
| float     | float   | F             |
| double    | double  | D             |
| boolean   | bool    | Z             |
| byte      | byte    | B             |
| char      | char    | C             |
| short     | short   | S             |
| Object    | DynJClass | L...;       |

## Performance

The project includes built-in benchmarking capabilities:

```csharp
using (var benchmark = new Benchmark.Benchmark("OperationName")) {
    // Your code here
}
// Automatically logs execution time
```

### Benchmark Results

The `Plotting` project can analyze performance over multiple iterations:

```bash
cd JICCLR/Plotting
dotnet run
```

This generates performance graphs showing:
- Per-iteration execution time
- First iteration overhead
- Total execution time trends

## Project Structure

```
JIC/
├── JICJVM/                           # Java-side implementation
│   ├── src/main/java/
│   │   └── me/ddayo/jic/
│   │       ├── JIC.java              # Main Java entry point with native method declarations
│   │       └── example/
│   │           ├── Test.java         # Example Java test class
│   │           └── AbstractTest.java # Abstract base class for testing(inherit)
│   │
│   ├── integrated.txt                # CLR assembly paths configuration
│   └── spent.txt                     # Benchmark timing data
│
├── JICNative/                        # Native C++ bridge
│   ├── library.cpp                   # JNI implementation and CLR hosting
│   └── library.h                     # Native function declarations
│
├── JICCLR/                           # .NET-side implementation
│   ├── JIC/                          # Core interop library
│   │   ├── ManagedEntry.cs           # CLR entry point and assembly management
│   │   ├── MainEntryAttribute.cs     # Attribute for marking entry methods
│   │   ├── JClassAttribute.cs        # Attribute for Java class mapping
│   │   └── JNI/
│   │       ├── JClass.cs             # Java class representation and invocation
│   │       └── JMethod.cs            # Java method invocation and marshalling
│   │
│   ├── Example/                      # Usage examples
│   │   ├── Program.cs                # Example entry point with benchmarks
│   │   ├── Test.cs                   # C# wrapper for Java Test class
│   │   └── Integer.cs                # C# wrapper for java.lang.Integer
│   │
│   ├── Benchmark/                    # Performance utilities
│   │   ├── Benchmark.cs              # Benchmark timing implementation
│   │   └── BenchmarkManager.cs       # Benchmark results aggregation
│   │
│   └── Plotting/                     # Performance visualization
│       └── Program.cs                # Benchmark execution and graph generation
```

## How It Works

1. **Initialization**: Java application loads the native library and calls `clrMain()`
2. **CLR Hosting**: Native layer initializes the .NET runtime using DNNE
3. **Assembly Loading**: Specified .NET assemblies are loaded from `integrated.txt`
4. **Entry Discovery**: Methods marked with `[MainEntry]` attribute are located
5. **Execution**: Entry methods are invoked, enabling C# code execution
6. **Interop**: C# code can dynamically invoke Java methods through `DynJClass`

## Advanced Features

### Dynamic Method Invocation

```csharp
dynamic javaObj = new MyJavaClass();
// Methods are resolved at runtime
var result = javaObj.anyJavaMethod(param1, param2);
```

### Method Chaining

```csharp
dynamic obj = new TestClass();
obj.method1(10)
   .method2(20)
   .method3(30);
```

### Constructor Overloading

```csharp
// Automatically selects the correct Java constructor
var obj1 = new MyJavaClass();           // No-arg constructor
var obj2 = new MyJavaClass(123);        // int constructor
var obj3 = new MyJavaClass("text");     // String constructor
```

## Limitations

- Currently optimized for macOS (paths in code are hardcoded for my macbook)
- Requires manual path configuration for different environments
- No support for Java arrays or generic types (yet)
- Exception handling between Java and C# needs improvement

## Contributing

Contributions are welcome! Areas for improvement:
- Cross-platform path resolution
- Enhanced type marshalling
- Better exception propagation
- Support for collections and arrays
- Improved documentation and examples

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Something similar with [node-java](https://github.com/joeferner/node-java) project
- Uses [DNNE](https://github.com/AaronRobinsonMSFT/DNNE) for .NET native exports
- Built with JNI (Java Native Interface)

## Contact

For questions, issues, or contributions, please open an issue on the GitHub repository.

---

*Note: This project was developed in 2023 and is not actively maintained, but the code remains available for those needing JVM-CLR interoperability without IPC.*

> special thanks to claude that helps to generate readme :)
