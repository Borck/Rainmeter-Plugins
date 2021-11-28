using System.Text.RegularExpressions;



namespace PluginFeedParser {
  public readonly struct SyndicationFeedInfo {
    public readonly string URL;
    public readonly string Prefix;

    private static readonly Regex MatchUrl = new Regex( @"^(.*)=(\[[0-9a-zA-Z]*\])$" );



    public SyndicationFeedInfo(string url, string prefix) {
      URL = url;
      Prefix = prefix;
    }



    public static SyndicationFeedInfo FromTokens(string feedString) {
      var match = MatchUrl.Match( feedString );
      return match.Success
               ? new SyndicationFeedInfo( match.Groups[1].Value, match.Groups[2].Value )
               : new SyndicationFeedInfo( feedString, default );
    }
  }
}