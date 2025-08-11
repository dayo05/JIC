package me.ddayo.jic;

import java.io.PrintWriter;
import java.util.ArrayList;

public class JIC {
    private static native void clrMain();

    public static void main(String[] args) {
        System.load("/Users/dayo/JIC/JICNative/cmake-build-debug/libJICNative.dylib");
        clrMain();
    }

    // Used on native!
    public static String getSignature(String jClass, String method, String sig) {
        try {
            var parseSig = new ArrayList<Class<?>>();
            int mode = 0;
            int sp = 0;
            for (int i = 0; i < sig.length(); i++) {
                if (mode == 1) {
                    if (sig.charAt(i) == ';') {
                        mode = 0;
                        parseSig.add(Class.forName(sig.substring(sp, i).replace('/', '.')));
                    }
                } else switch (sig.charAt(i)) {
                    case 'L' -> {
                        sp = i + 1;
                        mode = 1;
                    }
                    case 'I' -> parseSig.add(int.class);
                    case 'F' -> parseSig.add(float.class);
                    case 'D' -> parseSig.add(double.class);
                    case 'B' -> parseSig.add(byte.class);
                    case 'J' -> parseSig.add(long.class);
                    case 'S' -> parseSig.add(short.class);
                    case 'C' -> parseSig.add(char.class);
                    case 'Z' -> parseSig.add(boolean.class);
                }
            }

            if(method.equals("<init>")) {
                var methods = Class.forName(jClass).getConstructors();
                ctorIter: for(var x : methods) {
                    if(x.getParameterCount() != parseSig.size()) continue;
                    var types = x.getParameterTypes();
                    for (int i = 0; i < parseSig.size(); i++)
                        if (types[i] != parseSig.get(i)) continue ctorIter;
                    return "(" + sig + ")V";
                }
                return "()V";
            }
            else {
                var methods = Class.forName(jClass).getMethods();
                methodIter:
                for (var x : methods) {
                    if (!x.getName().equals(method)) continue;
                    if (x.getParameterCount() != parseSig.size()) continue;
                    var types = x.getParameterTypes();
                    for (int i = 0; i < parseSig.size(); i++)
                        if (types[i] != parseSig.get(i)) continue methodIter;
                    var returnType = x.getReturnType();

                    var c = "(" + sig + ")";
                    if (returnType.equals(void.class)) c += "V";
                    else if (returnType.equals(int.class)) c += "I";
                    else if (returnType.equals(long.class)) c += "J";
                    else if (returnType.equals(boolean.class)) c += "Z";
                    else if (returnType.equals(short.class)) c += "S";
                    else if (returnType.equals(char.class)) c += "C";
                    else if (returnType.equals(float.class)) c += "F";
                    else if (returnType.equals(double.class)) c += "D";
                    else c += "L" + returnType.getTypeName() + ";";
                    return c;
                }
            }
        } catch (Exception e) {
            try {
                var fss = new PrintWriter("jss.ot");
                fss.append(e.toString());
                e.printStackTrace(fss);
                fss.flush();
                fss.close();
            } catch(Exception ee) {}
        }
        return "";
    }
}