namespace ForbiddenWordsFinder;

internal sealed class PauseController
{
    private readonly ManualResetEventSlim resumeEvent = new(true);

    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
        resumeEvent.Reset();
    }

    public void Resume()
    {
        IsPaused = false;
        resumeEvent.Set();
    }

    public void WaitIfPaused(CancellationToken cancellationToken)
    {
        resumeEvent.Wait(cancellationToken);
    }
}
