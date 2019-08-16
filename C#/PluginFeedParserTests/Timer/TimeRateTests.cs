using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace PluginFeedParser.Timer {
  [TestClass]
  public class TimeRateTests {
    [TestMethod]
    public void Test1() {
      var tr = new TimeRate( TimeSpan.FromDays( 365 ) );

      var test1 = tr.Test();
      var test2 = tr.Test();

      Assert.IsTrue( test1 );
      Assert.IsFalse( test2 );
    }



    [TestMethod]
    public void Test2() {
      TimeRate.TryParse( "0:30", out var tr );

      var test1 = tr.Test();
      var test2 = tr.Test();

      Assert.IsTrue( test1 );
      Assert.IsFalse( test2 );
    }



    [TestMethod]
    public void Test3() {
      var ts = TimeSpan.FromSeconds( 1 );
      var tr = new TimeRate( ts );

      var test1 = tr.Test();
      var test2 = tr.Test();
      Thread.Sleep( ts );
      var test3 = tr.Test();

      Assert.IsTrue( test1 );
      Assert.IsFalse( test2 );
      Assert.IsTrue( test3 );
    }



    [TestMethod]
    public void Test4() {
      var ts = TimeSpan.FromMilliseconds( 500 );
      var tr = new TimeRate( ts );

      var test1 = tr.Test();
      Thread.Sleep( ts );
      Thread.Sleep( ts );
      var test2 = tr.Test();
      var test3 = tr.Test();

      Assert.IsTrue( test1 );
      Assert.IsTrue( test2 );
      Assert.IsFalse( test3 );
    }
  }
}