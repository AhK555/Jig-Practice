using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;

public class CircleMoveJig : DrawJig
{
    private Point3d _position;
    private readonly double _radius;

    public CircleMoveJig(Point3d startPoint, double radius)
    {
        _position = startPoint;
        _radius = radius;
    }

    protected override SamplerStatus Sampler(JigPrompts prompts)
    {
        var options = new JigPromptPointOptions(
            "\nMove circle and specify location: ");

        PromptPointResult result = prompts.AcquirePoint(options);

        if (result.Status != PromptStatus.OK)
            return SamplerStatus.Cancel;

        if (result.Value.IsEqualTo(
                _position,
                new Tolerance(1e-6, 1e-6)))
        {
            return SamplerStatus.NoChange;
        }

        _position = result.Value;

        return SamplerStatus.OK;
    }

    protected override bool WorldDraw(WorldDraw draw)
    {
        Point3d p1 = _position + Vector3d.XAxis * _radius;
        Point3d p2 = _position + Vector3d.YAxis * _radius;
        Point3d p3 = _position - Vector3d.XAxis * _radius;

        draw.Geometry.Circle(p1, p2, p3);

        return true;
    }

    public Point3d Position => _position;
    public double Radius => _radius;
}