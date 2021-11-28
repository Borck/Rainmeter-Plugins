using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace PluginFeedParser {
  /// <summary>
  ///   Zusammenfassungsbeschreibung für SyndicationFeedInfoTests
  /// </summary>
  [TestClass]
  public class SyndicationFeedInfoTests {
    public SyndicationFeedInfoTests() {
      //
      // TODO: Konstruktorlogik hier hinzufügen
      //
    }



    private TestContext testContextInstance;

    /// <summary>
    ///   Ruft den Textkontext mit Informationen über
    ///   den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
    /// </summary>
    public TestContext TestContext {
      get { return testContextInstance; }
      set { testContextInstance = value; }
    }

    #region Zusätzliche Testattribute

    //
    // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
    //
    // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Mit TestCleanup können Sie nach jedem Test Code ausführen.
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //

    #endregion



    [TestMethod]
    public void FromToken() {
      var url = "https://url.de/rss.php?feed=ATOM1.0=[abc]";
      var info = SyndicationFeedInfo.FromTokens( url );
      Assert.AreEqual( "[abc]", info.Prefix );
      Assert.AreEqual( "https://url.de/rss.php?feed=ATOM1.0", info.URL );
    }
  }
}