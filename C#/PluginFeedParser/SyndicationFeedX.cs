using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;



namespace PluginNewsfeedParser {
  internal static class SyndicationFeedX {
    public static IEnumerable<SyndicationItem>
      GetItemsOrderByPublishDateBeginNewest(this IEnumerable<SyndicationFeed> feeds) {
      return feeds
             .SelectMany( feed => feed.Items )
             .OrderByDescending( item => item.PublishDate );
    }



    public static SyndicationFeed ReadFeed(string url) {
      using (var reader = XmlReader.Create( url )) {
        return SyndicationFeed.Load( reader );
      }
    }
  }
}