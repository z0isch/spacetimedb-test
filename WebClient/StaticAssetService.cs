using System;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public struct StaticAssets
{
    public string[] Words { get; set; }
    public SubstringFrequency[] SubstringsOrderedByFrequency { get; set; }
}

public struct SubstringFrequency
{
    public string Substring { get; set; }
    public int Frequency { get; set; }
}

public class StaticAssetService
{
    public static StaticAssets? StaticAssets { get; private set; }
    public static async Task<StaticAssets> GetStaticAssets(HttpClient httpClient)
    {
        if (StaticAssets == null)
        {
            var responses = await Task.WhenAll(httpClient.GetAsync("words/words.txt.gz"), httpClient.GetAsync("words/histogram.csv.gz"));
            using (var wordsGzipStream = new GZipStream(await responses[0].Content.ReadAsStreamAsync(), CompressionMode.Decompress))
            using (var substringsGzipStream = new GZipStream(await responses[1].Content.ReadAsStreamAsync(), CompressionMode.Decompress))
            using (var wordsReader = new StreamReader(wordsGzipStream))
            using (var substringsReader = new StreamReader(substringsGzipStream))
            {
                var unzippedContents = await Task.WhenAll(wordsReader.ReadToEndAsync(), substringsReader.ReadToEndAsync());
                StaticAssets = new StaticAssets
                {
                    Words = unzippedContents[0].Split("\n"),
                    SubstringsOrderedByFrequency = unzippedContents[1].Split("\n")
                        .Select(s => s.Split(",") switch
                        {
                            [var substring, var frequency] => new SubstringFrequency
                            {
                                Substring = substring,
                                Frequency = int.Parse(frequency)
                            },
                            _ => throw new Exception("Invalid substring frequency")

                        })
                        .ToArray()
                };
            }
        }
        return StaticAssets.Value;
    }
}