using OpenTK.Mathematics;

public class Instance
{
    public Vector3 Position { get; set; }
    public Vector3 Scale { get; set; }
    public Quaternion Rotation { get; set; }
    public required Model Model { get; set; }
}

public partial class Stage
{
    public List<Instance> Instances = [];
}