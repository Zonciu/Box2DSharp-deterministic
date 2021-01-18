namespace Testbed.Abstractions
{
    public interface IInput
    {
        bool IsKeyDown(KeyCodes key);

        bool IsKeyPressed(KeyCodes key);

        bool IsKeyUp(KeyCodes key);

        bool IsMouseDown(MouseButton button);

        bool IsMousePressed(MouseButton button);

        bool IsMouseUp(MouseButton button);
    }
}