using System.Linq;
using System.ServiceModel.Syndication;



namespace PluginNewsfeedParser {
  internal class SyndicationFeedProvider {
    private readonly SyndicationItem[] _items;

    public int ItemsCount => _items.Length;



    public SyndicationFeedProvider(SyndicationItem[] items) {
      _items = items;
    }



    public string GetString(int i, MeasureType type) {
      var item = _items[i];
      switch (type) {
        case MeasureType.TITLE:
          return item.Title.Text;
        case MeasureType.URL:
          return item.Links.FirstOrDefault()?.Uri.OriginalString;
        default:
          return null;
      }
    }
  }
}