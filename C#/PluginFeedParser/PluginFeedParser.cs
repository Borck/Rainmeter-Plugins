/*
  Copyright (C) 2014 Birunthan Mohanathas

  This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using Rainmeter;

// Overview: This example demonstrates a basic implementation of a parent/child
// measure structure. In this particular example, we have a "parent" measure
// which contains the values for the options "ValueA", "ValueB", and "ValueC".
// The child measures are used to return a specific value from the parent.

// Use case: You could, for example, have a "main" parent measure that queries
// information some data set. The child measures can then be used to return
// specific information from the data queried by the parent measure.

// Sample skin:
/*
    [Rainmeter]
    Update=1000
    BackgroundMode=2
    SolidColor=000000

    [mParent]
    Measure=Plugin
    Plugin=FeedParser.dll
    ValueA=111
    ValueB=222
    ValueC=333
    Type=A

    [mChild1]
    Measure=Plugin
    Plugin=FeedParser.dll
    ParentName=mParent
    Type=B

    [mChild2]
    Measure=Plugin
    Plugin=FeedParser.dll
    ParentName=mParent
    Type=C

    [Text]
    Meter=STRING
    MeasureName=mParent
    MeasureName2=mChild1
    MeasureName3=mChild2
    X=5
    Y=5
    W=200
    H=55
    FontColor=FFFFFF
    Text="mParent: %1#CRLF#mChild1: %2#CRLF#mChild2: %3"
*/



namespace PluginNewsfeedParser {
  internal class Measure {
    internal MeasureType Type = MeasureType.NONE;
    internal int ItemIndex { get; private set; }


    internal readonly IntPtr Skin;
    internal readonly string Name;



    internal Measure(API api) {
      Skin = api.GetSkin();
      Name = api.GetMeasureName();
    }



    internal virtual void Dispose() { }



    internal virtual void Reload(API api, ref double maxValue) {
      ItemIndex = api.ReadInt( "FeedIndex", -1 );

      var type = api.ReadString( "Type", "" ).ToLowerInvariant();
      switch (type) {
        case "title":
          Type = MeasureType.TITLE;
          break;
        case "url":
          Type = MeasureType.URL;
          break;
        case "":
        case null:
          Type = MeasureType.NONE;
          break;
        default:
          Plugin.Log( API.LogType.Error, "Type=" + type + " not valid" );
          break;
      }
    }



    internal virtual double Update() {
      return 0.0;
    }



    internal virtual string GetString() {
      return "";
    }
  }



  internal class ParentMeasure : Measure {
    private const int DEFAULT_LOAD_TIMEOUT_MILLIS = 30000;

    // This list of all parent measures is used by the child measures to find their parent.
    internal static Dictionary<Tuple<IntPtr, string>, ParentMeasure> ParentMeasures =
      new Dictionary<Tuple<IntPtr, string>, ParentMeasure>();


    private string[] _urls = new string[0];
    private IDictionary<string, SyndicationFeed> _feeds = new Dictionary<string, SyndicationFeed>();
    private SyndicationItem[] _items = new SyndicationItem[0];
    private TimeSpan _timeout;

    public bool Parallel { get; private set; }
    public int UpdateRate { get; private set; }
    private int _updateCount = 0;



    internal ParentMeasure(API api) : base( api ) {
      ParentMeasures.Add( new Tuple<IntPtr, string>( Skin, Name ), this );
    }



    internal override void Dispose() {
      ParentMeasures.Remove( new Tuple<IntPtr, string>( Skin, Name ) );
    }



    internal override void Reload(API api, ref double maxValue) {
      base.Reload( api, ref maxValue );

      _urls = ReadUrls( api );
      _timeout = TimeSpan.FromMilliseconds( api.ReadInt( "Timeout", DEFAULT_LOAD_TIMEOUT_MILLIS ) );
      Parallel = api.ReadInt( "Parallel", 0 ) == 1;
      UpdateRate = Math.Max( api.ReadInt( "UpdateRate", 600 ), 1 );
    }



    private string[] ReadUrls(API api) {
      var urlsRaw = api.ReadString( "Url", "" ).Split( new[] {' '}, StringSplitOptions.RemoveEmptyEntries );
      var urls = urlsRaw.Distinct().ToArray();

      var duplicatesCount = urlsRaw.Length - urls.Length;
      if (duplicatesCount > 0) {
        Plugin.Log( API.LogType.Warning, $"[{Name}] {duplicatesCount} duplicates found in Url" );
      }

      return urls;
    }



    internal override double Update() {
      var doUpdate = _updateCount == 0;
      _updateCount = ( _updateCount + 1 ) % UpdateRate;
      if (!doUpdate) {
        return 0.0;
      }

      var urls = _urls;
      if (urls == null) {
        return 0;
      }

      Plugin.Log( API.LogType.Debug, $"Fetching {urls.Length} feed(s): {string.Join( ", ", urls )}" );

      var fetches = Parallel && urls.Length > 1
                      ? ReadFeedsParallel( urls )
                      : ReadFeeds( urls );

      var feeds = CompleteFeeds( fetches );
      var items = feeds.Values.GetItemsOrderByPublishDateBeginNewest();
      _feeds = feeds;
      _items = items.ToArray();

      return 0.0;
    }



    private IEnumerable<Tuple<string, SyndicationFeed>> ReadFeeds(string[] urls) {
      var fetches = new List<Tuple<string, SyndicationFeed>>();
      foreach (var url in urls) {
        var feed = RunWithTimeout( () => ReadFeed( url ), _timeout, default(SyndicationFeed) );
        fetches.Add( new Tuple<string, SyndicationFeed>( url, feed ) );
      }

      return fetches;
    }



    private static T RunWithTimeout<T>(Func<T> action, TimeSpan timeout, T defaultValue) {
      var tokenSource = new CancellationTokenSource();
      var task = Task.Factory.StartNew( action, tokenSource.Token ); //Execute a long running process

      //Check the task is delaying
      if (Task.WaitAll( new Task[] {task}, timeout )) {
        return task.Result;
      }

      // timeout
      //Cancel the task
      tokenSource.Cancel();

      task.Wait(); //Waiting for the task to throw OperationCanceledException
      return defaultValue;
    }



    internal IEnumerable<Tuple<string, SyndicationFeed>> ReadFeedsParallel(string[] urls) {
      var fetches = new List<Tuple<string, Task<SyndicationFeed>>>();
      foreach (var url in urls) {
        var fetch = Task.Factory.StartNew( () => ReadFeed( url ) );
        fetches.Add( new Tuple<string, Task<SyndicationFeed>>( url, fetch ) );
      }

      if (!Task.WaitAll( fetches.Select( fetch => fetch.Item2 ).ToArray() as Task<SyndicationFeed>[], _timeout ))
        Plugin.Log( API.LogType.Warning, $"Not all feeds could be fetched, urls: {string.Join( ", ", urls )} " );

      return fetches.Select(
        fetch =>
          new Tuple<string, SyndicationFeed>(
            fetch.Item1,
            GetResultNowOrDefault( fetch.Item2 ) ) );
    }



    private static SyndicationFeed ReadFeed(string url) {
      try {
        return SyndicationFeedX.ReadFeed( url );
      }
      catch (Exception e) {
        Plugin.Log( API.LogType.Error, "Failed to read feed from url: " + e.Message );
        return null;
      }
    }



    private static T GetResultNowOrDefault<T>(Task<T> task) {
      return task.IsCompleted
               ? task.Result
               : default(T);
    }



    private Dictionary<string, SyndicationFeed> CompleteFeeds(IEnumerable<Tuple<string, SyndicationFeed>> fetches) {
      var result = new Dictionary<string, SyndicationFeed>();
      foreach (var fetch in fetches) {
        var url = fetch.Item1;
        var feed = fetch.Item2;
        if (feed != null) {
          result.Add( url, feed );
          continue;
        }

        if (_feeds.TryGetValue( url, out var predFeed )) {
          result.Add( url, predFeed );
        }
      }

      return result;
    }



    internal string GetString(int itemIndex, MeasureType type) {
      var items = _items;
      if (itemIndex < 0 || itemIndex >= items.Length) {
        return "";
      }

      var item = items[itemIndex];
      switch (type) {
        case MeasureType.TITLE:
          return item.Title.Text;
        case MeasureType.URL:
          if (item.Links.Any()) {
            return item.Links[0].Uri.OriginalString;
          }

          break;
      }

      return "";
    }



    internal override string GetString() {
      return GetString( ItemIndex, Type );
    }
  }



  internal class ChildMeasure : Measure {
    private ParentMeasure _parentMeasure;

    internal ChildMeasure(API api) : base( api ) { }



    internal override void Reload(API api, ref double maxValue) {
      base.Reload( api, ref maxValue );

      var parentName = api.ReadString( "Parent", "" );
      //parentName = parentName.Substring( 1, parentName.Length - 2 );
      var skin = api.GetSkin();

      // Find parent using name AND the skin handle to be sure that it's the right one.
      _parentMeasure = GetParentMeasure( parentName, skin );

      if (_parentMeasure == null) {
        Plugin.Log( API.LogType.Error, "Parent=" + parentName + " not valid" );
      }
    }



    private static ParentMeasure GetParentMeasure(string parentName, IntPtr skin) {
      return ParentMeasure.ParentMeasures.TryGetValue( new Tuple<IntPtr, string>( skin, parentName ),
                                                       out var parentMeasure )
               ? parentMeasure
               : null;
    }



    internal override double Update() {
      return 0.0;
    }



    internal override string GetString() {
      return _parentMeasure?.GetString( ItemIndex, Type ) ?? "";
    }
  }



  public static class Plugin {
    private const string MESSAGE_PREFIX = "FeedParser.dll: ";
    private static IntPtr _stringBuffer = IntPtr.Zero;



    private static Measure AsMeasure(IntPtr measurePtr) {
      return (Measure) GCHandle.FromIntPtr( measurePtr ).Target;
    }



    [DllExport]
    public static void Initialize(ref IntPtr data, IntPtr rm) {
      var api = new API( rm );

      var measure =
        string.IsNullOrEmpty( api.ReadString( "Parent", default(string) ) )
          ? new ParentMeasure( api )
          : new ChildMeasure( api ) as Measure;

      data = GCHandle.ToIntPtr( GCHandle.Alloc( measure ) );
    }



    [DllExport]
    public static void Finalize(IntPtr data) {
      AsMeasure( data ).Dispose();
      GCHandle.FromIntPtr( data ).Free();

      if (_stringBuffer != IntPtr.Zero) {
        Marshal.FreeHGlobal( _stringBuffer );
        _stringBuffer = IntPtr.Zero;
      }
    }



    [DllExport]
    public static void Reload(IntPtr data, IntPtr rm, ref double maxValue) {
      AsMeasure( data ).Reload( new API( rm ), ref maxValue );
    }



    [DllExport]
    public static double Update(IntPtr data) {
      return AsMeasure( data ).Update();
    }



    [DllExport]
    public static IntPtr GetString(IntPtr data) {
      var measure = (Measure) GCHandle.FromIntPtr( data ).Target;
      if (_stringBuffer != IntPtr.Zero) {
        Marshal.FreeHGlobal( _stringBuffer );
        _stringBuffer = IntPtr.Zero;
      }

      var value = measure.GetString();
      if (value != null) {
        _stringBuffer = Marshal.StringToHGlobalUni( value );
      }

      return _stringBuffer;
    }



    internal static void Log(API.LogType type, string message) {
      API.Log( type, MESSAGE_PREFIX + message );
    }
  }
}