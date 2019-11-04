using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using MetroFramework;
using Newtonsoft.Json;
using static History_11.Global;

namespace History_11
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public static event EventHandler GroupUpdated;
        public bool loaded = false;
        

        public string mBorderFP = $"{Environment.CurrentDirectory}\\Borders\\ManitobaBorder.json";
        public string nsBorderFP = $"{Environment.CurrentDirectory}\\Borders\\NSBorder.json";
        public string nbBorderFP = $"{Environment.CurrentDirectory}\\Borders\\NBBorder.json";
        public string oBorderFP = $"{Environment.CurrentDirectory}\\Borders\\OntarioBorder.json";
        public string nwtBorderFP = $"{Environment.CurrentDirectory}\\Borders\\northwest_territoriesBorder.json";
        public string peiBorderFP = $"{Environment.CurrentDirectory}\\Borders\\Prince_Edward_IslandBorder.json";
        public string skwnBorderFP = $"{Environment.CurrentDirectory}\\Borders\\SaskatchewanBorder.json";
        public string yknBorderFP = $"{Environment.CurrentDirectory}\\Borders\\YukonBorder.json";
        public string albBorderFP = $"{Environment.CurrentDirectory}\\Borders\\AlbertaBorder.json";
        public string nflBorderFP = $"{Environment.CurrentDirectory}\\Borders\\NewfoundlandBorder.json";
        public string qbcBorderFP = $"{Environment.CurrentDirectory}\\Borders\\QuebecBorder.json";
        public string bcBorderFP = $"{Environment.CurrentDirectory}\\Borders\\British_ColumbiaBorder.json";
        public string lbrBorderFP = $"{Environment.CurrentDirectory}\\Borders\\labradorBorder.json";

        internal Borders ManitobaBorder;
        internal Borders NSBorder;
        internal Borders NBBorder;
        internal Borders oBorder;
        internal Borders nwtBorder;
        internal Borders peiBorder;
        internal Borders skwnBorder;
        internal Borders yknBorder;
        internal Borders bcBorder;
        internal Borders qbcBorder;
        internal Borders albBorder;
        internal Borders nflBorder;
        internal Borders lbrBorder;
        
        private delegate void SafeCallDelegate();

        internal List<GMapProvider> mapList = new List<GMapProvider>();
        internal bool init = false;
        internal PointLatLng latLng;

        public Form1()
        {
            InitializeComponent();
            addMuseum1.Hide();
            comboBox2.Items.Add("Any");
            foreach (var item in Enum.GetValues(typeof(Provinces)))
                comboBox2.Items.Add(item);
            comboBox2.SelectedItem = "Any";
            ManitobaBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(mBorderFP));
            NSBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(nsBorderFP));
            NBBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(nbBorderFP));
            oBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(oBorderFP));
            nwtBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(nwtBorderFP));
            peiBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(peiBorderFP));
            skwnBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(skwnBorderFP));
            yknBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(yknBorderFP));
            bcBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(bcBorderFP));
            qbcBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(qbcBorderFP));
            albBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(albBorderFP));
            nflBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(nflBorderFP));
            lbrBorder = JsonConvert.DeserializeObject<Borders>(File.ReadAllText(lbrBorderFP));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadingScreen1.Hide();
            //load json
            MuseumData data = JsonConvert.DeserializeObject<MuseumData>(File.ReadAllText(mFilePath));
            currentData = data;
            //plot points
            PlotMuseums();
        }
        private void PlotMuseums()
        {
            gMapControl1.MarkersEnabled = true;
            currentMap = gMapControl1;
            GMapOverlay markerOverlay = new GMapOverlay("markers");
            CurrentMarkerOverlay = markerOverlay;
            gMapControl1.OnMarkerClick += handleMarker;
            markerOverlay.IsZoomSignificant = true;
            int emptycounter = 0;
            Type t = currentData.GetType();
            var pA = t.GetProperties();
            //var test1 = pA[1];
            gMapControl1.Overlays.Add(markerOverlay);
            List<string> MuseumTypes = new List<string>();
            foreach (var museumObj in pA)
            {
                var gMuseumData = museumObj.GetValue(currentData);
                var props = gMuseumData.GetType().GetProperties();
                List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                foreach (var museum in final)
                {
                    if (!MuseumTypes.Contains(museum.Type)) { MuseumTypes.Add(museum.Type); }
                    if (museum.LatLang == PointLatLng.Empty) { emptycounter++; }
                    else
                    {
                        GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);                        
                        if(museum.Link != null) { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}\nLink: {museum.Link}"; }
                        else { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}"; }
                        marker.ToolTip = new GMapToolTip(marker);
                        marker.ToolTip.Stroke = Pens.Black;
                        markerOverlay.Markers.Add(marker);
                    }
                }
            }
            mTypes = MuseumTypes.ToArray();
            comboBox3.SelectedItem = comboBox3.Items.Add("Any");
            foreach (var item in MuseumTypes)
                comboBox3.Items.Add(item);
            Console.WriteLine(emptycounter);
            loaded = true;
            CurrentMarkerOverlay.Polygons.Clear();
            polyAblerta();
            polyBritishColumbia();
            polyManitoba();
            polyNewBrunswick();
            polyNFLD();
            polyNWTeritory();
            polyNovaScotia();
            polyOntario();
            polyPEI();
            polyQuebec();
            polySaskatchewan();
            polyYukon();
        }

        private void handleMarker(GMapMarker item, MouseEventArgs e)
        {
            string[] items = item.ToolTipText.Split('\n');
            if(items.Last().Contains("Link:"))
            {
                string url = items.Last().Replace("Link: ", "");
                Process.Start(url);
            }
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            mapList = GMapProviders.List;
            foreach (var item in mapList)
                comboBox1.Items.Add(item);
            gMapControl1.MapProvider = GMapProviders.GoogleHybridMap;
            comboBox1.SelectedItem = GMapProviders.GoogleHybridMap;

            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 5000;
            gMapControl1.Zoom = 3.5;
            gMapControl1.Position = new PointLatLng(52.9089020477703, -98.7890625);
            gMapControl1.OnPositionChanged += posHandle;
            gMapControl1.DragButton = MouseButtons.Right;

            gMapControl1.MouseWheelZoomEnabled = true;
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            gMapControl1.ShowCenter = false;
            init = true;
        }
        
        private void posHandle(PointLatLng point)
        {
            latLng = point;
        }
        
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = mapList[comboBox1.SelectedIndex];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadingScreen1.Show();
            loadingScreen1.BringToFront();
            Thread t = new Thread(GetMuseumData);
            t.Start();
        }
        private static Task<int> GetResult1Async()
        {
            return Task.FromResult(0);
        }

        private static Task<long> GetResult2Async()
        {
            return Task.FromResult(0L);
        }
        private async void GetMuseumData()
        {
            Continuing = true;
            Task<AlbertaM> r0 = getAlbertaMuseums();
            Task<BritishColumbiaM> r1 = getBritishColumbiaMuseums();
            Task<ManitobaM> r2 = getManitobaMuseums();
            Task<NewBrunswickM> r3 = getNewBrunswickMuseums();
            Task<NewFoundLandandLabadorM> r4 = getNewFoundLandandLabadorMuseums();
            Task<NorthWestTeritoriesM> r5 = getNorthWestTeritoriesMuseums();
            Task<NovaScotiaM> r6 = getNovaScotiaMuseums();
            Task<OntarioM> r7 = getOntarioMuseums();
            Task<PEI_M> r8 = getPEI_Museums();
            Task<QuebecM> r9 = getQuebecMuseums();
            Task<SaskatchewanM> r10 = getSaskatchewanMuseums();
            Task<YukonM> r11 = getYukonMuseums();

            await Task.WhenAll(r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11);

            //get museum data
            if (r0.Status == TaskStatus.RanToCompletion && r1.Status == TaskStatus.RanToCompletion && r2.Status == TaskStatus.RanToCompletion && r3.Status == TaskStatus.RanToCompletion && r4.Status == TaskStatus.RanToCompletion && r5.Status == TaskStatus.RanToCompletion && r6.Status == TaskStatus.RanToCompletion && r7.Status == TaskStatus.RanToCompletion && r8.Status == TaskStatus.RanToCompletion && r9.Status == TaskStatus.RanToCompletion && r10.Status == TaskStatus.RanToCompletion && r11.Status == TaskStatus.RanToCompletion)
            {
                Console.WriteLine("All provences museums set");
                MuseumData data = new MuseumData()
                {
                    AlbertaM = r0.Result,
                    BritishColumbiaM = r1.Result,
                    ManitobaM = r2.Result,
                    NewBrunswickM = r3.Result,
                    NewFoundLandandLabadorM = r4.Result,
                    NorthWestTeritoriesM = r5.Result,
                    NovaScotiaM = r6.Result,
                    OntarioM = r7.Result,
                    PEIM = r8.Result,
                    QuebecM = r9.Result,
                    SaskatchewanM = r10.Result,
                    YukonM = r11.Result
                };
                currentData = data;
                string jsondata = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(mFilePath, jsondata);

            }
            else { Console.WriteLine("A Task was not finnsihed"); }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox3.SelectedItem.ToString() != "Any")
            {
                Type t = currentData.GetType();
                var pA = t.GetProperties();
                CurrentMarkerOverlay.Markers.Clear();
                foreach (var museumObj in pA)
                {
                    var gMuseumData = museumObj.GetValue(currentData);
                    var props = gMuseumData.GetType().GetProperties();
                    List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                    foreach (var museum in final.Where(x => x.Type == comboBox3.SelectedItem.ToString()))
                    {
                        GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                        if (museum.Link != null) { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}\nLink: {museum.Link}"; }
                        else { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}"; }
                        marker.ToolTip = new GMapToolTip(marker);
                        marker.ToolTip.Stroke = Pens.Black;
                        CurrentMarkerOverlay.Markers.Add(marker);
                    }
                }
            }
            else
            {
                Type t = currentData.GetType();
                var pA = t.GetProperties();
                CurrentMarkerOverlay.Markers.Clear();
                foreach (var museumObj in pA)
                {
                    var gMuseumData = museumObj.GetValue(currentData);
                    var props = gMuseumData.GetType().GetProperties();
                    List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                    foreach (var museum in final)
                    {
                        GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                        if (museum.Link != null) { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}\nLink: {museum.Link}"; }
                        else { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}"; }
                        marker.ToolTip = new GMapToolTip(marker);
                        marker.ToolTip.Stroke = Pens.Black;
                        CurrentMarkerOverlay.Markers.Add(marker);
                    }
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox2.SelectedItem.ToString() != "Any")
            {

                Provinces SelectedProvince;
                Enum.TryParse<Provinces>(comboBox2.SelectedItem.ToString(), out SelectedProvince);
                if (!checkBox1.Checked)
                {
                    switch (SelectedProvince)
                    {
                        case Provinces.Alberta:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyAblerta();
                            break;
                        case Provinces.BritishColumbia:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyBritishColumbia();
                            break;
                        case Provinces.Manitoba:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyManitoba();
                            break;
                        case Provinces.NewBrunswick:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyNewBrunswick();
                            break;
                        case Provinces.NewFoundLandandLabador:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyNFLD();
                            break;
                        case Provinces.NorthWestTeritories:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyNWTeritory();
                            break;
                        case Provinces.NovaScotia:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyNovaScotia();
                            break;
                        case Provinces.Ontario:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyOntario();
                            break;
                        case Provinces.PEI:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyPEI();
                            break;
                        case Provinces.Quebec:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyQuebec();
                            break;
                        case Provinces.Saskatchewan:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polySaskatchewan();
                            break;
                        case Provinces.Yukon:
                            CurrentMarkerOverlay.Polygons.Clear();
                            polyYukon();
                            break;
                    }
                }
                CurrentMarkerOverlay.Markers.Clear();
                Type t = currentData.GetType();
                var pA = t.GetProperty(SelectedProvince.ToString() + "M");
                var gMuseumData = pA.GetValue(currentData);
                var props = gMuseumData.GetType().GetProperties();
                List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                var epoly = CurrentMarkerOverlay.Polygons.Where(x => x.Name == SelectedProvince.ToString());

                foreach (var museum in final)
                {
                    foreach(var poly in epoly)
                    {
                        if (poly.IsInside(museum.LatLang))
                        {
                            GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                            marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}";
                            marker.ToolTip = new GMapToolTip(marker);
                            CurrentMarkerOverlay.Markers.Add(marker);
                        }
                    }
                }
                
            }
            else if(loaded)
            {
                CurrentMarkerOverlay.Markers.Clear();
                int emptycounter = 0;
                foreach (var museumObj in currentData.GetType().GetProperties())
                {
                    var gMuseumData = museumObj.GetValue(currentData);
                    var props = gMuseumData.GetType().GetProperties();
                    List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                    foreach (var museum in final)
                    {
                        if (museum.LatLang == PointLatLng.Empty) { emptycounter++; }
                        else
                        {
                            GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                            marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}";
                            marker.ToolTip = new GMapToolTip(marker);
                            CurrentMarkerOverlay.Markers.Add(marker);
                        }
                    }
                }
                if (!checkBox1.Checked)
                {
                    CurrentMarkerOverlay.Polygons.Clear();
                    polyAblerta();
                    polyBritishColumbia();
                    polyManitoba();
                    polyNewBrunswick();
                    polyNFLD();
                    polyNWTeritory();
                    polyNovaScotia();
                    polyOntario();
                    polyPEI();
                    polyQuebec();
                    polySaskatchewan();
                    polyYukon();
                }
            }
        }
        internal void polyAblerta()
        {
            
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in albBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "Alberta");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Cyan));
            polygon.Stroke = new Pen(Color.Cyan, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyBritishColumbia()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in bcBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "BritishColumbia");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.DeepSkyBlue));
            polygon.Stroke = new Pen(Color.DeepSkyBlue, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyManitoba()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in ManitobaBorder.polygonpoints)
                points.Add(new PointLatLng(point[1], point[0]));
            GMapPolygon polygon = new GMapPolygon(points, "Manitoba");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.LawnGreen));
            polygon.Stroke = new Pen(Color.LawnGreen, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyNewBrunswick()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in NBBorder.polygonpoints)
                points.Add(new PointLatLng(point[1], point[0]));
            GMapPolygon polygon = new GMapPolygon(points, "NewBrunswick");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.HotPink));
            polygon.Stroke = new Pen(Color.HotPink, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyNFLD()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in nflBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "NewFoundLandandLabador");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.LightPink));
            polygon.Stroke = new Pen(Color.LightPink, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);

            List<PointLatLng> points2 = new List<PointLatLng>();
            foreach (var point in lbrBorder.polygonpoints)
                points2.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon2 = new GMapPolygon(points2, "NewFoundLandandLabador");
            polygon2.Fill = new SolidBrush(Color.FromArgb(50, Color.LightPink));
            polygon2.Stroke = new Pen(Color.LightPink, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon2);

        }
        internal void polyNWTeritory()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in nwtBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "NorthWestTeritories");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Blue));
            polygon.Stroke = new Pen(Color.Blue, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyNovaScotia()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in NSBorder.polygonpoints)
                points.Add(new PointLatLng(point[1], point[0]));
            GMapPolygon polygon = new GMapPolygon(points, "NovaScotia");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
            polygon.Stroke = new Pen(Color.Red, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyOntario()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in oBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "Ontario");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Orange));
            polygon.Stroke = new Pen(Color.Orange, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyPEI()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in peiBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "PEI");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Purple));
            polygon.Stroke = new Pen(Color.Purple, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyQuebec()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in qbcBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "Quebec");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Purple));
            polygon.Stroke = new Pen(Color.Purple, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polySaskatchewan()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in skwnBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "Saskatchewan");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.LightGreen));
            polygon.Stroke = new Pen(Color.LightGreen, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }
        internal void polyYukon()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var point in yknBorder.polygonpoints)
                points.Add(new PointLatLng(point[0], point[1]));
            GMapPolygon polygon = new GMapPolygon(points, "Yukon");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Lime));
            polygon.Stroke = new Pen(Color.Lime, 1);
            CurrentMarkerOverlay.Polygons.Add(polygon);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Type t = currentData.GetType();
            var pA = t.GetProperties();
            CurrentMarkerOverlay.Markers.Clear();
            foreach (var museumObj in pA)
            {
                var gMuseumData = museumObj.GetValue(currentData);
                var props = gMuseumData.GetType().GetProperties();
                List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                foreach (var museum in final.Where(x => x.Name.ToLower().Contains(textBox1.Text.ToLower())))
                {
                    GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                    if (museum.Link != null) { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}\nLink: {museum.Link}"; }
                    else { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}"; }
                    marker.ToolTip = new GMapToolTip(marker);
                    marker.ToolTip.Stroke = Pens.Black;
                    CurrentMarkerOverlay.Markers.Add(marker);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                CurrentMarkerOverlay.Polygons.Clear();
            }
            else
            {
                polyAblerta();
                polyBritishColumbia();
                polyManitoba();
                polyNewBrunswick();
                polyNFLD();
                polyNWTeritory();
                polyNovaScotia();
                polyOntario();
                polyPEI();
                polyQuebec();
                polySaskatchewan();
                polyYukon();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            addMuseum1.Show();
            addMuseum1.BringToFront();
            addMuseum1.Size = new Size(320, 250);
        }
    }
}
