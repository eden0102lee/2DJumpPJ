namespace Hollow.Player
{
    public interface IHeroState
    {
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
    }
}
