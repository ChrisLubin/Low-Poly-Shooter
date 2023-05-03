using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnityTimer : MonoBehaviour
{
    private static List<TimerData> _timers = new();
    List<TimerData> _timersToRemove = new();

    public static Task Delay(float timer)
    {
        TimerData timerData = new(timer);
        _timers.Add(timerData);
        return timerData.Tcs.Task;
    }

    private void Update()
    {
        _timersToRemove.Clear();

        foreach (TimerData timerData in UnityTimer._timers)
        {
            timerData.DecrementTimer(Time.deltaTime);

            if (timerData.Timer <= 0f)
            {
                timerData.Tcs.SetResult(true);
                _timersToRemove.Add(timerData);
            }
        }

        foreach (TimerData timerData in _timersToRemove)
        {
            UnityTimer._timers.Remove(timerData);
        }
    }

    private class TimerData
    {
        public float Timer { get; private set; }
        public TaskCompletionSource<bool> Tcs { get; private set; }

        public TimerData(float timer)
        {
            this.Timer = timer;
            this.Tcs = new();
        }

        public void DecrementTimer(float decrementAmount) => this.Timer -= decrementAmount;
    }
}
