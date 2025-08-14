# JIC - Java Interop for C#

JNI와 DNNE를 사용하여 Java와 .NET (C#) 간의 고성능 브리지를 제공하며, JVM과 CLR 환경 간의 원활한 상호 운용성을 가능하게 합니다.

## 개요

JIC (Java Interop for C#)는 Java와 C# 코드가 IPC (Inter-Process Communication) 없이 직접 상호 작용할 수 있도록 하는 네이티브 브리지를 제공합니다. 이 프로젝트는 CLR을 JVM 프로세스에 임베드하여 최소한의 오버헤드로 언어 경계를 넘나드는 동적 메서드 호출을 가능하게 합니다.

## 아키텍처

```
┌─────────────┐     JNI      ┌──────────────┐     DNNE     ┌─────────────┐
│   Java/JVM  │ ◄──────────► │ Native Layer │ ◄──────────► │   C#/.NET   │
│   (Entry)   │              │    (C++)     │              │    (CLR)    │
└─────────────┘              └──────────────┘              └─────────────┘
```

### 주요 구성 요소

- **JICJVM**: JVM 진입점을 포함한 Java 측 구현
- **JICNative**: JNI를 사용한 네이티브 C++ 브리지 레이어
- **JICCLR**: CLR 호스팅을 포함한 .NET 측 구현
  - **JIC**: JNI 바인딩을 포함한 핵심 상호 운용 라이브러리
  - **Example**: 사용 패턴을 보여주는 샘플 구현
  - **Benchmark**: 성능 측정 유틸리티

## 기능

- **양방향 통신**: C#에서 Java 메서드 호출 및 그 반대로도 가능
- **동적 메서드 해결**: 자동 메서드 시그니처 감지 및 바인딩
- **타입 마샬링**: Java와 .NET 타입 간의 자동 변환
- **성능 최적화**: 직렬화 오버헤드 없는 직접적인 네이티브 호출
- **벤치마킹 지원**: 내장 성능 측정 도구

## 요구 사항

- Java Development Kit (JDK) 17 이상
- .NET 6.0 SDK 이상
- C++17을 지원하는 C++ 컴파일러
- CMake 3.26 이상 (네이티브 라이브러리용)
- 운영 체제: macOS, Linux, 또는 Windows

## 설치

### 1. 네이티브 브리지 빌드

```bash
cd JICNative
mkdir build && cd build
cmake ..
make
```

### 2. .NET 컴포넌트 빌드

```bash
cd JICCLR
dotnet build -c Release
```

### 3. Java 컴포넌트 빌드

```bash
cd JICJVM
./gradlew build
```

### 4. 통합 구성

컴파일된 .NET 어셈블리의 경로를 `JICJVM/integrated.txt`에 업데이트:
```
/path/to/your/JICCLR/Example/bin/Release/net6.0/Example.dll
```

## 사용법

### Java 측 - 진입점 정의

```java
public class JIC {
    private static native void clrMain();
    
    public static void main(String[] args) {
        System.load("/path/to/libJICNative.dylib");
        clrMain();
    }
}
```

### C# 측 - 상호 운용 클래스 생성

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
        // 이 메서드는 Java에서 호출됩니다
        dynamic javaObject = new MyJavaClass();
        javaObject.someJavaMethod(123);
        
        var result = javaObject.getResult();
        Console.WriteLine($"Java로부터의 결과: {result}");
    }
}
```

### 통합 예제

```csharp
// Java의 Integer 클래스를 위한 래퍼 정의
[JClass("java.lang")]
public class Integer : DynJClass {
    public Integer(int value) : base(value) { }
}

// 코드에서 사용
[MainEntry]
public static void Example() {
    dynamic integer = new Integer(42);
    double value = integer.doubleValue();
    Console.WriteLine($"double로 변환된 Integer: {value}");
}
```

## 타입 매핑

| Java 타입 | C# 타입 | JNI 시그니처 |
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

## 성능

프로젝트에는 내장 벤치마킹 기능이 포함되어 있습니다:

```csharp
using (var benchmark = new Benchmark.Benchmark("OperationName")) {
    // 여기에 코드 작성
}
// 실행 시간을 자동으로 로깅
```

### 벤치마크 결과

`Plotting` 프로젝트는 여러 반복에 대한 성능을 분석할 수 있습니다:

```bash
cd JICCLR/Plotting
dotnet run
```

다음을 보여주는 성능 그래프를 생성합니다:
- 반복당 실행 시간
- 첫 번째 반복 오버헤드
- 총 실행 시간 추세

## 프로젝트 구조

```
JIC/
├── JICJVM/                           # Java 측 구현
│   ├── src/main/java/
│   │   └── me/ddayo/jic/
│   │       ├── JIC.java              # 네이티브 메서드 선언을 포함한 메인 Java 진입점
│   │       └── example/
│   │           ├── Test.java         # 예제 Java 테스트 클래스
│   │           └── AbstractTest.java # 테스트용 추상 기본 클래스(상속)
│   │
│   ├── integrated.txt                # CLR 어셈블리 경로 구성
│   └── spent.txt                     # 벤치마크 타이밍 데이터
│
├── JICNative/                        # 네이티브 C++ 브리지
│   ├── library.cpp                   # JNI 구현 및 CLR 호스팅
│   └── library.h                     # 네이티브 함수 선언
│
├── JICCLR/                           # .NET 측 구현
│   ├── JIC/                          # 핵심 상호 운용 라이브러리
│   │   ├── ManagedEntry.cs           # CLR 진입점 및 어셈블리 관리
│   │   ├── MainEntryAttribute.cs     # 진입 메서드 표시용 속성
│   │   ├── JClassAttribute.cs        # Java 클래스 매핑용 속성
│   │   └── JNI/
│   │       ├── JClass.cs             # Java 클래스 표현 및 호출
│   │       └── JMethod.cs            # Java 메서드 호출 및 마샬링
│   │
│   ├── Example/                      # 사용 예제
│   │   ├── Program.cs                # 벤치마크를 포함한 예제 진입점
│   │   ├── Test.cs                   # Java Test 클래스를 위한 C# 래퍼
│   │   └── Integer.cs                # java.lang.Integer를 위한 C# 래퍼
│   │
│   ├── Benchmark/                    # 성능 유틸리티
│   │   ├── Benchmark.cs              # 벤치마크 타이밍 구현
│   │   └── BenchmarkManager.cs       # 벤치마크 결과 집계
│   │
│   └── Plotting/                     # 성능 시각화
│       └── Program.cs                # 벤치마크 실행 및 그래프 생성
```

## 작동 원리

1. **초기화**: Java 애플리케이션이 네이티브 라이브러리를 로드하고 `clrMain()`을 호출
2. **CLR 호스팅**: 네이티브 레이어가 DNNE를 사용하여 .NET 런타임을 초기화
3. **어셈블리 로딩**: `integrated.txt`에서 지정된 .NET 어셈블리를 로드
4. **진입점 발견**: `[MainEntry]` 속성으로 표시된 메서드를 찾음
5. **실행**: 진입 메서드가 호출되어 C# 코드 실행 가능
6. **상호 운용**: C# 코드가 `DynJClass`를 통해 Java 메서드를 동적으로 호출 가능

## 고급 기능

### 동적 메서드 호출

```csharp
dynamic javaObj = new MyJavaClass();
// 메서드는 런타임에 해결됨
var result = javaObj.anyJavaMethod(param1, param2);
```

### 메서드 체이닝

```csharp
dynamic obj = new TestClass();
obj.method1(10)
   .method2(20)
   .method3(30);
```

### 생성자 오버로딩

```csharp
// 올바른 Java 생성자를 자동으로 선택
var obj1 = new MyJavaClass();           // 인자 없는 생성자
var obj2 = new MyJavaClass(123);        // int 생성자
var obj3 = new MyJavaClass("text");     // String 생성자
```

## 제한 사항

- 현재 macOS에 최적화되어 있음 (코드의 경로가 제 맥북에 하드코딩되어 있음)
- 다른 환경에서는 수동 경로 구성 필요
- Java 배열이나 제네릭 타입 지원 안 됨 (아직)
- Java와 C# 간의 예외 처리 개선 필요

## 기여

기여를 환영합니다! 개선이 필요한 영역:
- 크로스 플랫폼 경로 해결
- 향상된 타입 마샬링
- 더 나은 예외 전파
- 컬렉션과 배열 지원
- 개선된 문서화 및 예제

## 라이선스

이 프로젝트의 모든 코드는 MIT 라이선스 하에 라이선스가 부여됩니다.

## 그외 기타

- [node-java](https://github.com/joeferner/node-java) 프로젝트와 유사한 부분이 있습니다
- .NET 네이티브 익스포트를 위해 [DNNE](https://github.com/AaronRobinsonMSFT/DNNE) 사용
- JNI (Java Native Interface)로 구축됨

## 연락처

질문, 이슈 또는 기여에 대해서는 GitHub 저장소에 이슈를 열어주세요.

---

*참고: 이 프로젝트는 2023년에 개발되었으며 활발히 유지 관리되지 않지만, IPC 없이 JVM-CLR 상호 운용성이 필요한 사람들을 위해 하드 깁숙한 곳에서 코드를 찾아서 올려봅니다.*

> readme 생성을 도와준 claude에게 특별히 감사합니다 :)
