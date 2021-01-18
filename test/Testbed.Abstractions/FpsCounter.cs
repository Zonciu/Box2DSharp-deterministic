using System.Diagnostics;

namespace Testbed.Abstractions
{
    public class FpsCounter
    {
        public float Ms = 0;

        public float Fps = 0;

        public int FrameCount = 0;

        private float _totalDiff = 0;

        private long _lastTimeDiff = 0;

        private long _lastTime = 0;

        private readonly Stopwatch _fpsTimer = Stopwatch.StartNew();

        public void Count()
        {
            FrameCount++;
            var now = _fpsTimer.ElapsedMilliseconds;
            _lastTimeDiff = now - _lastTime;
            _totalDiff += _lastTimeDiff;
            _lastTime = now;

            if (_totalDiff > 1000)
            {
                Ms = _totalDiff / FrameCount;
                Fps = FrameCount / _totalDiff * 1000;
                _totalDiff = 0;
                FrameCount = 0;
            }
        }
    }
}