namespace RiftLingo.Models;

public sealed record CaptureRegion(int X, int Y, int Width, int Height)
{
    public bool IsValid => Width >= 80 && Height >= 30;

    public override string ToString() => $"X {X}, Y {Y}, {Width} × {Height}";
}
