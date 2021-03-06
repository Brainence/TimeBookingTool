﻿using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TBT.App
{

    public class GlobalTimer
    {
        private DispatcherTimer _timer;
        private DateTime _cacheTickInterval;
        public GlobalTimer()
        {
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += Timer_Tick;
            _cacheTickInterval = DateTime.Now;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timerTick?.Invoke();
            if ((DateTime.Now.TimeOfDay - _cacheTickInterval.TimeOfDay).Minutes > 0)
            {
                CacheTimerTick?.Invoke();
                _cacheTickInterval = DateTime.Now;
            }
        }

        public async Task<bool> Start(int id)
        {
            if (_timer.IsEnabled) _timer.Stop();
            var data = await App.CommunicationService.GetAsJson($"TimeEntry/Start/{id}");
            if (data != null && JsonConvert.DeserializeObject<bool>(data))
            {
                _timer.Start();
                return true;
            }
            return false;
        }

        public async Task<bool> Stop(int id)
        {
            var data = await App.CommunicationService.GetAsJson($"TimeEntry/Stop/{id}");
            if (data != null && JsonConvert.DeserializeObject<bool>(data))
            {
                _timer.Stop();
                return true;
            }
            return false;
        }

        public void StartTimer()
        {
            if (_timer != null && !_timer.IsEnabled)
                _timer.Start();
        }

        public void StopTimer()
        {
            if (_timer != null && _timer.IsEnabled)
                _timer.Stop();
        }

        public event Action CacheTimerTick;
        private Action _timerTick;
        public event Action TimerTick
        {
            add
            {
                if (_timerTick != null)
                {
                    var subscribers = _timerTick.GetInvocationList();

                    foreach (var action in subscribers)
                        _timerTick -= action as Action;
                }
                _timerTick += value;
            }
            remove
            {
                _timerTick -= value;
            }
        }
    }
}
