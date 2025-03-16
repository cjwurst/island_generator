using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InvertibleInvoker
{
    readonly List<IInvertible> queue = new List<IInvertible>();
    List<IInvertible> buffer = new List<IInvertible>();

    public InvertibleInvoker(CallbackDirector director)
    {
        director.roundPassedEvent += CallbackDirector.ResponseToDispatcher(OnRoundPassed, float.PositiveInfinity);

        director.invokeRequestedEvent += Invoke;
        director.reverseRoundRequestedEvent += Reverse;
    }

    void OnRoundPassed(List<IInvertible> _) { PackageBuffer(); }

    void Invoke(IInvertible invertible)
    {
        buffer.Add(invertible);
        invertible.Do();
    }

    // *Reverse* goes back *count* rounds. *count = 1* brings the game back to the beginning of the current round.
    public void Reverse(int count = 1)
    {
        Assert.IsTrue(count > 0);
        if (buffer.Count > 0)
            PackageBuffer();
        int adjustedCount = Mathf.Min(queue.Count, count);
        for (int i = 0; i < adjustedCount; i++)
        {
            var command = queue.Pop();
            command.Undo();
        }
    }

    void PackageBuffer()
    {
        if (buffer.Count == 0) return;
        queue.Add(Helper.Compose(buffer));
        buffer = new List<IInvertible>();
    }
}
