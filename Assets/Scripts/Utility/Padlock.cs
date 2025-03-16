using System;

public class Padlock
{
    int lockCount = 0;
    public bool IsLocked { get => lockCount > 0; }

    public Action Lock()
    {
        lockCount++;
        Action unlock = () =>
        {
            lockCount--;
            unlock = () => { };
        };
        return unlock;
    }
}
