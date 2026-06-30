using System;
using System.Collections.Generic;

namespace Hollow.Player
{
    public class HeroStateMachine
    {
        public IHeroState CurrentState { get; private set; }
        public Type CurrentStateType => CurrentState?.GetType();

        private readonly Dictionary<Type, IHeroState> _states = new();

        public void RegisterState(IHeroState state)
        {
            _states[state.GetType()] = state;
        }

        public void Initialize(Type startingState)
        {
            ChangeState(startingState);
        }

        public void ChangeState(Type stateType)
        {
            if (!_states.TryGetValue(stateType, out var newState))
                throw new ArgumentException($"State {stateType.Name} is not registered.");

            if (CurrentState == newState)
                return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Tick() => CurrentState?.Tick();
        public void FixedTick() => CurrentState?.FixedTick();
    }
}
