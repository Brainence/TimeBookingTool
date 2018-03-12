using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TBT.App
{

    public class GlobalTimer
    {
        private DispatcherTimer _timer;
        private DateTime _cacheTickInterval;
        private DateTime _midnightTickTime;
        public GlobalTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += Timer_Tick;
            _cacheTickInterval = DateTime.Now;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timerTick?.Invoke();
            if((DateTime.Now.TimeOfDay - _cacheTickInterval.TimeOfDay).Minutes > 0)
            {
                CacheTimerTick?.Invoke();
                _cacheTickInterval = DateTime.Now;
            }
        }

        public async Task<bool> Start(int id)
        {
            try
            {
                if (_timer.IsEnabled) _timer.Stop();

                var result = JsonConvert.DeserializeObject<bool>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/Start/{id}"));

                if (result) _timer.Start();

                return await Task.FromResult(result);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> Stop(int id)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<bool>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/Stop/{id}"));

                if (result) _timer.Stop();

                return await Task.FromResult(result);
            }
            catch
            {
                return await Task.FromResult(false);
            }
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

                    for (int i = 0; i < subscribers.Length; i++)
                        _timerTick -= subscribers[i] as Action;
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
