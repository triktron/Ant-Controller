public struct ControllerInputStruct 
{
    public float Forward;
    public float Right;
    public float Rotate;
    public float Run;

    public ControllerInputStruct(float forward, float right, float rotate, float run)
    {
        Forward = forward;
        Right = right;
        Rotate = rotate;
        Run = run;
    }

    public static ControllerInputStruct Empty = new ControllerInputStruct(0, 0, 0, 0);
}
