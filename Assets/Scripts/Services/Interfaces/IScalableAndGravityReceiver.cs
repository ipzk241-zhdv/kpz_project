public interface IScalableAndGravityReceiver
{
    void OnTimeScaleChanged(float newScale);
}

public interface IGravityConstantReceiver
{
    void OnGravityConstantChanged(double G);
}