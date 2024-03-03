using Godot;

public interface IChunkGraphicModule
{
    void Draw(Data d);
    Node2D Node { get; }
}