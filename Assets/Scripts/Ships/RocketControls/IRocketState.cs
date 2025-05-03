public interface IRocketState
{
    void Enter(RocketController rocket);
    void Exit(RocketController rocket);
    void Update();
    void FixedUpdate();
    void OnGUI();
}