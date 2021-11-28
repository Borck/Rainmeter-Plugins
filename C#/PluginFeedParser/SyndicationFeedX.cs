using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using PluginFeedParser;



namespace PluginNewsfeedParser {
  internal static class SyndicationFeedX {
    private static readonly XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings {
      IgnoreComments = true
    };



    public static IEnumerable<SyndicationItem>
      GetItemsOrderByPublishDateBeginNewest(this IEnumerable<SyndicationFeed> feeds) {
      return feeds
             .SelectMany( feed => feed.Items )
             .OrderByDescending( item => item.PublishDate );
    }
    
    public static IEnumerable<Tuple<TKey, SyndicationItem>>
      GetItemsOrderByPublishDateBeginNewest<TKey>(this ICollection<KeyValuePair<TKey, SyndicationFeed>> feeds) {
      return feeds
             .SelectMany(feed => feed.Value.Items,
                         (feed, item) => new Tuple<TKey, SyndicationItem>( feed.Key, item ))
             .OrderByDescending( item => item.Item2.PublishDate );
    }



    public static SyndicationFeed ReadFeed(string url) {
      using (var reader = XmlReader.Create( url, DefaultXmlReaderSettings )) {
        return SyndicationFeed.Load( reader );
      }
    }
  }
}
