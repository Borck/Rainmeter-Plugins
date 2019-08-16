using System;



namespace PluginFeedParser.Timer {
  public class Ticker : IUpdateRate {
    private readonly object _lock = new object();
    private int _counter;
    public readonly int Interval;



    public Ticker(int interval, int initialCount) {
      Interval = Math.Max( interval, 1 );
      _counter = Math.Max( initialCount % Interval, 0 );
    }



    public Ticker(int interval) : this( interval, interval - 1 ) { }



    public bool Test() {
      lock (_lock) {
        _counter = ( _counter + 1 ) % Interval;
        return _counter == 0;
      }
    }



    public static bool TryParse(string s, out Ticker ticker) {
      var parsed = int.TryParse( s, out var interval );
      ticker = parsed
                 ? new Ticker( interval )
                 : default(Ticker);
      return parsed;
    }
  }
}