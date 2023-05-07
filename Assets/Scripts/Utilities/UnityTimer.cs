using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnityTimer : MonoBehaviour
{
    private static List<TimerData> _timers = new();
    List<TimerData> _timersToRemove = new();

    public static Task Delay(int timerMilliseconds)
    {
        TimerData timerData = new(timerMilliseconds);
        _timers.Add(timerData);
        return timerData.Tcs.Task;
    }

    private void FixedUpdate()
    {
        if (UnityTimer._timers.Count == 0) { return; }

        _timersToRemove.Clear();
        int decrementBy = (int)(Time.fixedDeltaTime * 1000);

        foreach (TimerData timerData in UnityTimer._timers)
        {
            timerData.DecrementTimer(decrementBy);

            if (timerData.Timer <= 0)
            {
                _timersToRemove.Add(timerData);
            }
        }

        foreach (TimerData timerData in _timersToRemove)
        {
            UnityTimer._timers.Remove(timerData);
            timerData.Tcs.SetResult(true);
        }
    }

    private class TimerData
    {
        public int Timer { get; private set; }
        public TaskCompletionSource<bool> Tcs { get; private set; }

        public TimerData(int timer)
        {
            this.Timer = timer;
            this.Tcs = new();
        }

        public void DecrementTimer(int decrementAmount) => this.Timer -= decrementAmount;
    }
}
