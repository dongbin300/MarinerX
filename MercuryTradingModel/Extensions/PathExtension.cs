namespace MercuryTradingModel.Extensions
{
    public static class PathExtension
    {
        public static string Down(this string path, params string[] downPaths)
        {
            return Path.Combine(path, Path.Combine(downPaths));
        }

        public static void TryCreate(this string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
            }
        }

        public static void TryCreateDirectory(this string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetDirectory(this string path)
        {
            string[] data = path.Split('\\');
            return path.Replace(data[^1], "");
        }

        public static string GetFileName(this string path)
        {
            string[] data = path.Split('\\');
            return data[^1];
        }

        public static string GetExtension(this string path)
        {
            return path[path.LastIndexOf('.')..];
        }

        public static string GetOnlyFileName(this string path)
        {
            string data = GetFileName(path);
            return data.Replace(GetExtension(path), "");
        }
    }
}
