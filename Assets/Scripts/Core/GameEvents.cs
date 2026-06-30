using System;

namespace Hollow.Core
{
    public static class GameEvents
    {
        public static event Action OnPlayerLanded;
        public static event Action OnDashStarted;

        public static void RaisePlayerLanded() => OnPlayerLanded?.Invoke();
        public static void RaiseDashStarted() => OnDashStarted?.Invoke();
    }
}
