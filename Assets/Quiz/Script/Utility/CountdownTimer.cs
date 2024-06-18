using System;
using UnityEngine;

namespace KanQuiz.Utility
{
    public class CountdownTimer
    {
        public float CurrentTime { get; private set; }
        public bool IsRunning { get; private set; }

        private float initialTime;
        private float tickInterval;
        private float elapsed;

        public float Progress => Mathf.Clamp(CurrentTime / initialTime, 0, 1) * initialTime;

        public event Action OnTimerStart;
        public event Action OnTimerStop;
        public event Action<int> OnTickEvent;


        public CountdownTimer(float initialTime, float tickInterval = 1f)
        {
            this.initialTime = initialTime;
            this.tickInterval = tickInterval;
        }

        public void Start()
        {
            CurrentTime = initialTime;
            elapsed = tickInterval;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop?.Invoke();
            }
        }

        public void Tick(float deltaTime)
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= deltaTime;
                elapsed += deltaTime;

                if (elapsed >= tickInterval)
                {
                    elapsed = 0;
                    OnTickEvent?.Invoke(Mathf.RoundToInt(CurrentTime));
                }
            }

            if (IsRunning && CurrentTime <= 0)
            {
                OnTickEvent?.Invoke(Mathf.RoundToInt(CurrentTime));
                Stop();
            }
        }

        public bool IsFinished => CurrentTime <= 0;
        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;
        public void Reset() => CurrentTime = initialTime;

        public void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }



    }
}
