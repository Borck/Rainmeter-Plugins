namespace PluginFeedParser {
  public static class ArrayX {
    public static bool TryGet<T>(this T[] array, int index, out T value) {
      var exists = index >= 0 && index < array.Length;
      value = exists
                ? array[index]
                : default(T);
      return exists;
    }
  }
}