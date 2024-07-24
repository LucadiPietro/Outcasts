namespace Common.Cutscenes.Commands
{
    using System.Collections;

    public interface ICinematicCommand
    {
        bool ShouldWaitEnd { get; }
        void Execute();
        IEnumerator ExecuteAwaitable();
        void FastForward();
    }
}
