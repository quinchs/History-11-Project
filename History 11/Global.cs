using GMap.NET;
using GMap.NET.WindowsForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace History_11
{
    class Global
    {
        public static event EventHandler ItemUpdated;
        public static event EventHandler ChangeBarMax;
        public static GMapControl currentMap { get; set; }
        public static GMapOverlay CurrentMarkerOverlay;
        public static Bitmap icon = new Bitmap($"{Environment.CurrentDirectory}\\mIcon.png");

        internal static int Count { get; private set; }
        internal static MuseumData currentData = new MuseumData();
        internal static bool Continuing = false;
        internal static string[] mTypes = new string[] { };
        internal static bool APILim = false;
        public enum Provinces
        {
            Alberta,
            BritishColumbia,
            Manitoba,
            NewBrunswick,
            NewFoundLandandLabador,
            NorthWestTeritories,
            NovaScotia,
            Ontario,
            PEI,
            Quebec,
            Saskatchewan,
            Yukon
        }

        internal static string mFilePath = $"{Environment.CurrentDirectory}\\MuseumData.json";
        public struct MuseumData
        {
            public AlbertaM AlbertaM { get; set; }
            public BritishColumbiaM BritishColumbiaM { get; set; }
            public ManitobaM ManitobaM { get; set; }
            public NewBrunswickM NewBrunswickM { get; set; }
            public NewFoundLandandLabadorM NewFoundLandandLabadorM { get; set; }
            public NorthWestTeritoriesM NorthWestTeritoriesM { get; set; }
            public NovaScotiaM NovaScotiaM { get; set; }
            public OntarioM OntarioM { get; set; }
            public PEI_M PEIM { get; set; }
            public QuebecM QuebecM { get; set; }
            public SaskatchewanM SaskatchewanM { get; set; }
            public YukonM YukonM { get; set; }
        }
        public struct Borders
        {
            public List<List<double>> polygonpoints { get; set; }
        }
        public struct GenerecMuseumList
        {
            public string Provence { get; set; }
            public List<Museum> Museums { get; set; }
            public int count { get; set; }
        }
        public struct Museum
        {
            public string Name { get; set; }
            public PointLatLng LatLang { get; set; }
            public string Town_city { get; set; }
            public string Region { get; set; }
            public string Type { get; set; }
            public string Summary { get; set; }
            public string Link { get; set; }
            public bool Custom { get; set; }
        }
        public class options
        {
            public Dictionary<string, string> location { get; set; }
        }
        public struct AlbertaM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct BritishColumbiaM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct ManitobaM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct NewBrunswickM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct NewFoundLandandLabadorM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct NorthWestTeritoriesM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct NovaScotiaM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct OntarioM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct PEI_M { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct QuebecM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct SaskatchewanM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public struct YukonM { public List<Museum> Museums { get; set; } public int count { get; set; } }
        public class UpdateEventArgs : EventArgs
        {
            public string item { get; set; }
            public int max { get; set; }
        }
        
        public static async Task<PointLatLng?> GetLatLongFromWikiURL(string href)
        {
            Regex r = new Regex("<a href=\"(.*?)\"\\s");
            var match = r.Match(href);
            var pUrl = match.Groups[1].Value;
            var fUrl = $"https://en.wikipedia.org{pUrl}";
            HttpClient c = new HttpClient();
            var req = await c.GetAsync(fUrl);
            var respCont = await req.Content.ReadAsStringAsync();
            Regex r2 = new Regex("title=\"Maps, aerial photos, and other data for this location\">(\\d*?\\.\\d*?).\\w (\\d*?\\.\\d*?).\\w<\\/span>");
            var match2 = r2.Match(respCont);
            if (match2.Groups.Count == 3)
            {
                try
                {
                    double lat = Convert.ToDouble(match2.Groups[1].Value);
                    double lng = Convert.ToDouble(match2.Groups[2].Value) * -1;
                    return new PointLatLng(lat, lng);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else if (respCont.Contains("<th scope=\"row\">Location</th>"))
            {
                Regex r3 = new Regex("Location<\\/th><td class=\"locality\"><a href=\"(.*?,|.*?)\".*?title=\"(.*?)\">(.*?,|.*?)<\\/a>,");
                var mtch = r3.Match(respCont);
                string location = mtch.Groups[2] + ", Canada";
                var point = await GetPosFromKeyword(location);
                return point;
            }
            else return null;
                   
        }
        #region SettingMuseums
        public static async Task<AlbertaM> getAlbertaMuseums()
        {
            AlbertaM m = new AlbertaM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Alberta";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            m.count = matchs.Count;
            Count = Count + m.count;
            Regex r2 = new Regex("\">(.*?)</a>");
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.AlbertaM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<BritishColumbiaM> getBritishColumbiaMuseums()
        {
            BritishColumbiaM m = new BritishColumbiaM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_British_Columbia";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.BritishColumbiaM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                   
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<ManitobaM> getManitobaMuseums()
        {
            ManitobaM m = new ManitobaM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Manitoba";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.ManitobaM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<NewBrunswickM> getNewBrunswickMuseums()
        {
            NewBrunswickM m = new NewBrunswickM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_New_Brunswick";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.NewBrunswickM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<NewFoundLandandLabadorM> getNewFoundLandandLabadorMuseums()
        {
            NewFoundLandandLabadorM m = new NewFoundLandandLabadorM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Newfoundland_and_Labrador";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.NewFoundLandandLabadorM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<NorthWestTeritoriesM> getNorthWestTeritoriesMuseums()
        {
            NorthWestTeritoriesM m = new NorthWestTeritoriesM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_the_Northwest_Territories";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.NorthWestTeritoriesM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                   
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<NovaScotiaM> getNovaScotiaMuseums()
        {
            NovaScotiaM m = new NovaScotiaM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Nova_Scotia";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.NovaScotiaM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<OntarioM> getOntarioMuseums()
        {
            OntarioM m = new OntarioM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Ontario";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.OntarioM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<PEI_M> getPEI_Museums()
        {
            PEI_M m = new PEI_M();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Prince_Edward_Island";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.PEIM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                   
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<QuebecM> getQuebecMuseums()
        {
            QuebecM m = new QuebecM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Quebec";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.QuebecM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<SaskatchewanM> getSaskatchewanMuseums()
        {
            SaskatchewanM m = new SaskatchewanM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Saskatchewan";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            var request = await c.GetAsync(link);
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("\">(.*?)</a>");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.SaskatchewanM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Type = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[5].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                    
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        public static async Task<YukonM> getYukonMuseums()
        {
            YukonM m = new YukonM();
            m.Museums = new List<Museum>();
            string link = "https://en.wikipedia.org/wiki/List_of_museums_in_Yukon";
            Regex r = new Regex(@"<tr>\n<td>(.*?)<\/a>\n<\/td>\n.*\/><\/a>\n<\/td>\n<td>(.*?)<\/a>\n<\/td>\n<td>\n<\/td>\n<td>(.*?)\n<\/td>\n<td>(.*?)\n<\/td><\/tr>");
            HttpClient c = new HttpClient();
            HttpResponseMessage request = c.GetAsync(link).Result;
            string responseContent = await request.Content.ReadAsStringAsync();
            var matchs = r.Matches(responseContent);
            Regex r2 = new Regex("<a.*?title=.*?\\\">(.*?)$");
            m.count = matchs.Count;
            Count = Count + m.count;
            ChangeBarMax.Invoke(null, new UpdateEventArgs() { max = Count });
            foreach (Match match in matchs)
            {
                if (!Continuing) { return currentData.YukonM; }
                string Title = Regex.Replace(match.Groups[1].Value, "<.*?>", String.Empty);
                string TownCity = Regex.Replace(match.Groups[2].Value, "<.*?>", String.Empty);
                string Region = "Yukon";
                string Type = Regex.Replace(match.Groups[3].Value, "<.*?>", String.Empty);
                string Summary = Regex.Replace(match.Groups[4].Value, "<.*?>", String.Empty);
                if (Title.Contains("<a href"))
                    Title = r2.Match(Title).Groups[1].Value;
                if (TownCity.Contains("<a href"))
                    TownCity = r2.Match(TownCity).Groups[1].Value;
                if (Region.Contains("<a href"))
                    Region = r2.Match(Region).Groups[1].Value;
                if (Type.Contains("<a href"))
                    Type = r2.Match(Type).Groups[1].Value;
                if (Summary.Contains("<a href"))
                    Summary = r2.Match(Summary).Groups[1].Value;
                if (Title.Contains("&amp;"))
                    Title.Replace("&amp;", "");
                if (TownCity.Contains("&amp;"))
                    TownCity.Replace("&amp;", "");
                if (Region.Contains("&amp;"))
                    Region.Replace("&amp;", "");
                if (Type.Contains("&amp;"))
                    Type.Replace("&amp;", "");
                if (Summary.Contains("&amp;"))
                    Summary.Replace("&amp;", "");
                PointLatLng? latlong;
                string checkHRef = match.Groups[1].Value;
                if (checkHRef.Contains("<a href") && !checkHRef.Contains("(page does not exist)")) { latlong = await GetLatLongFromWikiURL(checkHRef); }
                else { latlong = await GetPosFromKeyword(Title); }
                if (latlong != PointLatLng.Empty && latlong.HasValue && latlong != null)
                {
                    Regex rLink = new Regex("<a href=\"(.*?)\"\\s");
                    var lMatch = rLink.Match(checkHRef);
                    var pUrl = lMatch.Groups[1].Value;
                    var fUrl = $"https://en.wikipedia.org{pUrl}";

                    Museum msm = new Museum()
                    {
                        Name = Title,
                        Region = Region,
                        Summary = Summary,
                        Town_city = TownCity,
                        Type = Type,
                        LatLang = latlong.Value,
                        Link = fUrl,
                        Custom = false
                    };
                    m.Museums.Add(msm);
                   
                    Console.WriteLine($"Checked {Title} val: {latlong.HasValue.ToString()}");
                }
                ItemUpdated.Invoke(null, new UpdateEventArgs()
                {
                    item = Title
                });
            }
            return m;
        }
        #endregion
        public static async Task<PointLatLng?> GetPosFromKeyword(string searchWord)
        {
            GeocodingProvider gp = GMap.NET.MapProviders.GMapProviders.OpenStreetMap as GeocodingProvider;

            if (gp != null) 
            {
                try
                {
                    GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;
                   
                    PointLatLng? pt = gp.GetPoint(searchWord, out status);


                    if (status == GeoCoderStatusCode.OK && pt.HasValue)
                    {
                        HttpClient c = new HttpClient();
                        var tmp = await c.GetAsync($"http://api.geonames.org/countryCodeJSON?lat={pt.Value.Lat}&lng={pt.Value.Lng}&username=google");
                        var rsp = await tmp.Content.ReadAsStringAsync();
                        if (rsp.Contains("countryName\":\"Canada"))
                            return pt;
                        if (rsp.Contains("\"countryCode\":\"\""))
                        {
                            return null;
                        }
                        else
                        {
                            if (!APILim) { return await posfind(searchWord); }
                            else { return null; }
                        }
                    }
                    else
                    {
                        if (!APILim) { return await posfind(searchWord); }
                        else { return null; }
                    }
                }
                catch(Exception ex) { return null; }
            }
            else
            {
                return PointLatLng.Empty;
            }
        }
        private static async Task<PointLatLng?> posfind(string searchWord)
        {
            HttpClient c2 = new HttpClient();
            var req1 = await c2.GetAsync($"http://google.com/search?q={searchWord.Replace(' ', '+')}");
            Regex r3 = new Regex("<span class=\".*?\">(\\d|.*? [a-z A-Z]*?, [a-z A-Z]*?, \\w{2} .{3} .{3})<\\/span>");
            string cont = await req1.Content.ReadAsStringAsync();
            if (cont.Contains("Our systems have detected unusual traffic from your computer network.") && Continuing == true)
            {
                var m = MessageBox.Show("Unable to get Museum data, API Limit Reached. Continue without google?", "Error", MessageBoxButtons.YesNo);
                if(m == DialogResult.No) { Continuing = false; }
                else
                {
                    APILim = true;
                }
            }
            var match4 = r3.Match(cont);
            if (match4.Groups[1].Value != "")
            {
                Regex r4 = new Regex("<span class=\".*?\">(.*?, .*?),.*?$");
                var name = r4.Match(match4.Groups[1].Value);
                HttpClient clint = new HttpClient();
                options op = new options()
                {
                            location = new Dictionary<string, string>()
                            {
                                {"street", $"{name}" },
                                {"city", "" },
                                {"state", "" },
                                {"postalCode", "" },
                                {"adminArea1", "CA" }
                            }
                };
                //clint.DefaultRequestHeaders.Add("key", "lYrP4vF3Uk5zgTiGGuEzQGwGIVDGuy24");
                //clint.DefaultRequestHeaders.Add("callback", "callback_json5");
                string json = JsonConvert.SerializeObject(op);
                //clint.DefaultRequestHeaders.Add("json", json);
                var re = await clint.GetAsync($"https://www.mapquestapi.com/geocoding/v1/address?key=lYrP4vF3Uk5zgTiGGuEzQGwGIVDGuy24&json={json}&callback=callback_json5");
                string respon = await re.Content.ReadAsStringAsync();
                Regex rTemp = new Regex("{\\\"lat\\\":(.*?),\\\"lng\\\":(.*?)}");
                var mtc = rTemp.Match(respon);
                double lat = Convert.ToDouble(mtc.Groups[1].Value);
                double lng = Convert.ToDouble(mtc.Groups[2].Value);
                return new PointLatLng(lat, lng);
            }
            else { return PointLatLng.Empty; }
        }
    }
}
