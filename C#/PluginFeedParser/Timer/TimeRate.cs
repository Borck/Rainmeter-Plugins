using System;
using System.Diagnostics;



namespace PluginFeedParser.Timer {
  public class TimeRate : IUpdateRate {
    private readonly object _lock = new object();
    private readonly TimeSpan _interval;
    private DateTime _nextTick;



    public TimeRate(TimeSpan interval, DateTime lastTick) {
      _interval = interval;
      _nextTick = lastTick + interval;
    }



    public TimeRate(TimeSpan interval) : this( interval, DateTime.Now - interval ) { }



    //    6 --> 6.00:00:00
    //    6:12 --> 06:12:00
    //    6:12:14 --> 06:12:14
    //    6:12:14:45 --> 6.12:14:45
    //    6.12:14:45 --> 6.12:14:45
    //    6:12:14:45.3448 --> 6.12:14:45.3448000
    //    6:12:14:45,3448: Bad Format
    //    6:34:14:45: Overflow
    public static bool TryParse(string s, out TimeRate timeRate) {
      var parsed = TimeSpan.TryParse( s, out var span );
      timeRate = parsed
                   ? new TimeRate( span )
                   : default(TimeRate);
      return parsed;
    }



    public bool Test() {
      lock (_lock) {
        var now = DateTime.Now;
        if (now < _nextTick) {
          Debug.WriteLine( $"Time tick, now {ToString( now )}, next: {ToString( _nextTick )}" );
          return false;
        }

        var timeOverflow = now - _nextTick;
        var factor = timeOverflow.Ticks / _interval.Ticks + 1;
        _nextTick = _nextTick.AddTicks( _interval.Ticks * factor );
        //API.Log( API.LogType.Notice, "Next tick: " + _nextTick );
        Debug.WriteLine( $"Time's up, now {ToString( now )}, next: {ToString( _nextTick )}" );
      }

      return true;
    }



    private static string ToString(DateTime dateTime) {
      return dateTime.ToString( "HH:mm:ss.ffffzzz" );
    }
  }
}