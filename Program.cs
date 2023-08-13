using HtmlAgilityPack;

var downloadFolder = @""; //Enter a path to download folder
var archiveSourceFolder = @""; //Enter a path to a folder with html files from VK archive

Directory.CreateDirectory(downloadFolder);

DirectoryInfo d = new(archiveSourceFolder);

List<string> pictureUrls = new();

foreach (var file in d.GetFiles("*.html"))
{
    var htmlDoc = new HtmlDocument();
    htmlDoc.Load(file.FullName);

    var urls = htmlDoc.DocumentNode
        ?.SelectNodes("//a[@class='attachment__link' and @href]")
        ?.Select(node => node.Attributes["href"].Value)
        ?.Where(url => url.Contains(".jpg") || url.Contains(".jpeg") || url.Contains(".png"))
        ?.ToList();

    if (urls?.Count > 0)
    {
        pictureUrls.AddRange(urls);
    }
}

Console.WriteLine("Found " + pictureUrls.Count + " pictures.");

using (var client = new HttpClient())
{
    for (int i = 0; i < pictureUrls.Count; i++)
    {
        Console.WriteLine("Downloading " + (i + 1) + "/" + pictureUrls.Count);

        using var s = client.GetStreamAsync(pictureUrls[i]);
        var pictureName = pictureUrls[i][pictureUrls[i].LastIndexOf("/")..pictureUrls[i].IndexOf('?')];

        using var fs = new FileStream(Path.Combine(downloadFolder, pictureName[1..]), FileMode.OpenOrCreate);

        try
        {
            s.Result.CopyTo(fs);
        } 
        catch (Exception ex)
        {
            Console.WriteLine(pictureUrls[i]);
            Console.WriteLine(ex.Message);
        }
    }
}

Console.WriteLine("Done!");
Console.ReadLine();