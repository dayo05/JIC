namespace JIC; 

[AttributeUsage(AttributeTargets.Class)]
public class JClassAttribute: Attribute {
    public string Package { get; }
    public string? EntryPoint { get; }
    
    public JClassAttribute(string package) {
        Package = package;
    }

    public JClassAttribute(string package, string entryPoint): this(package) {
        EntryPoint = entryPoint;
    }
}