using System.Text;

namespace PriceLoaderWeb.Utils;

public static class EncodingDetector
{
    public static Encoding Detect(byte[] bytes)
    {
        if (bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF)
            return Encoding.UTF8;

        return Encoding.GetEncoding("windows-1251");
    }
}
