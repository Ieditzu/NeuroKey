using UnityEngine;

public static class MobileTouchInput
{
    private static Vector2 move;
    private static bool jumpRequested;

    public static Vector2 Move => move;

    public static void SetMove(Vector2 value)
    {
        move = Vector2.ClampMagnitude(value, 1f);
    }

    public static void RequestJump()
    {
        jumpRequested = true;
    }

    public static bool ConsumeJumpRequest()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    public static void ResetMove()
    {
        move = Vector2.zero;
    }
}
