using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;



namespace RssTest {
  /// <summary>
  ///   Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
    }



    private void Button_Click(object sender, RoutedEventArgs e) {
      XmlReader reader = XmlReader.Create( Url.Text );
      SyndicationFeed feed = SyndicationFeed.Load( reader );
      reader.Close();
      FeedContent.Items.Clear();
      foreach (SyndicationItem item in feed.Items) {
        string subject = item.Title.Text;
        FeedContent.Items.Add( subject );
      }
    }
  }
}