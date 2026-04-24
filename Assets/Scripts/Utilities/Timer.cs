using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Utilities {
    ///<summary>Basic timer functionality can be give more advanced things like repeating timers or basic stopwatches</summary>
    public abstract class Timer {

        ///<summary>Used to provide _time with value when starting/resetting timer</summary>
        [SerializeField] protected float _initialTime;

        ///<summary>Internal time value - ticks toward 0</summary>
        [SerializeField] protected float _time;

        ///<summary>Is timer running (used for property drawer)</summary>
        [SerializeField, HideInInspector] private bool _isRunning = false;

        ///<summary>Is timer running (will Update() do anything)</summary>
        public bool IsRunning { get => _isRunning; protected set => _isRunning = value; }

        ///<summary>Invoked upon calling Timer::Start()</summary>
        public Action OnTimerStart = delegate { };

        ///<summary>Invoked upon calling Timer::Stop() or when timer finishes</summary>
        public Action OnTimerStop = delegate { };

        ///<summary>Initialises timer but does not start it</summary>
        ///<param name="initialTime">Initial time for timer</param>
        protected Timer(float initialTime) {
            this._initialTime = initialTime;
            IsRunning = false;
        }

        ///<summary>Sets Time to InitialTime and allows Update to tick internal state</summary>
        ///<summary>Invokes OnTimerStart</summary>
        public void Start() {
            _time = _initialTime;
            if (!IsRunning) {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        ///<summary>Stops Update from ticking - does not reset internal Time value</summary>
        ///<summary>Invokes OnTimerStop</summary>
        public void Stop() {
            if (IsRunning) {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        ///<summary>Resumes timer - does not reset internal Time value</summary>
        public void Resume() => IsRunning = true;

        ///<summary>Pauses Timer - does not reset internal Time value</summary>
        public void Pause() => IsRunning = false;

        ///<summary>Progresses timer if IsRunning is true</summary>
        ///<param name="deltaTime">Amount to progress timer by</param>
        public abstract void Update(float deltaTime);

        ///<summary>Gets timer progress as a percent - 0% on start, 100% on end</summary>
        ///<returns>Normalized progress in range [0,1]</returns>
        public abstract float Progress();
    }

    ///<summary>Timer that counts down from initial time towards 0, returns progress and allows user to get remaining time</summary>
    [Serializable]
    public class CountDownTimer : Timer {

        ///<summary>Initialises timer but does not start it</summary>
        ///<param name="initialTime">Initial time for timer</param>
        public CountDownTimer(float initialTime) : base(initialTime) { }

        ///<summary>Progresses timer if IsRunning is true - moving _time towards 0</summary>
        ///<summary>Will automatically call Stop() upon _time hitting 0</summary>
        ///<param name="deltaTime">Amount to progress timer by</param>
        public override void Update(float deltaTime) {
            if (IsRunning && _time > 0f) {
                _time = Mathf.Max(_time - deltaTime, 0f);
            }

            if (IsRunning && _time == 0f) {
                Stop();
            }
        }

        ///<summary>Remaining time left on timer - not valid if not running</summary>
        public float RemainingTime => _time;

        ///<summary>Value timer was started with</summary>
        public float InitialTime => _initialTime;

        ///<summary>Is the timer finished ticking</summary>
        public bool IsFinished => _time == 0f;

        ///<summary>Sets _time back to _initialTime - does not restart timer</summary>
        public void Reset() => _time = _initialTime;

        ///<summary>Sets _time back to _initialTime (setting to newTime first)</summary>
        ///<summary>Will restart timer ticking if startTimer is true</summary>
        ///<param name="newTime">New timer duration</param>
        ///<param name="startTimer">Start timer after setting new time</param>
        public void Reset(float newTime, bool startTimer = true) {
            _initialTime = newTime;
            Reset();
            if (startTimer) {
                Start();
            }
        }

        ///<summary>Gets timer progress as a percent - 0% on start, 100% on end</summary>
        ///<returns>Normalized progress in range [0,1]</returns>
        public override float Progress() => 1f - _time / _initialTime;
    }

    ///<summary>Stopwatch used to record time duration after starting</summary>
    [Serializable]
    public class StopwatchTimer : Timer {

        ///<summary>Creates a new stopwatch initialised to 0</summary>
        public StopwatchTimer() : base(0f) { }

        ///<summary>Progresses stopwatch duration</summary>
        ///<param name="deltaTime">Amount to progress timer by</param>
        public override void Update(float deltaTime) {
            if (IsRunning) {
                _time += deltaTime;
            }
        }

        ///<summary>Resets _time but keeps ticking</summary>
        public void Reset() {
            _time = 0f;
        }

        ///<summary>Not valid for a stopwatch - always returns 0</summary>
        ///<returns>Always returns 0 as progress is not a valid construct</returns>
        public override float Progress() => 0.0f;

        ///<summary>Gets duration stopwatch has been running</summary>
        ///<returns>Duration</returns>
        public float GetTime() => _time;
    }
}