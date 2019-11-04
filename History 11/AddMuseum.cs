using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Controls;
using static History_11.Global;
using Newtonsoft.Json;
using System.IO;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;

namespace History_11
{
    public partial class AddMuseum : MetroFramework.Controls.MetroUserControl
    {
        public AddMuseum()
        {
            InitializeComponent();
        }

        private void AddMuseum_Load(object sender, EventArgs e)
        {
            foreach(var item in Enum.GetNames(typeof(Global.Provinces)))
                comboBox1.Items.Add(item);
            foreach (var item2 in Global.mTypes)
                comboBox2.Items.Add(item2);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != ""&& textBox2.Text != "" && textBox4.Text != "" && textBox5.Text != "")
            {
                if (comboBox2.SelectedItem != null && comboBox1.SelectedItem != null)
                {
                    double lat;
                    double lng;
                    Uri uriResult;
                    bool result = Uri.TryCreate(textBox6.Text, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
                    if (!result) { MessageBox.Show("Invalad link!"); return; }
                    try
                    {
                        lat = Convert.ToDouble(textBox5.Text);
                        lng = Convert.ToDouble(textBox4.Text);
                    }
                    catch { MessageBox.Show("The latitude and Longitude are not in the correct format!"); return; }

                    Global.Museum museum = new Global.Museum()
                    {
                        LatLang = new GMap.NET.PointLatLng(lat, lng),
                        Name = textBox1.Text,
                        Town_city = textBox2.Text,
                        Region = comboBox1.SelectedItem.ToString(),
                        Type = comboBox2.SelectedItem.ToString(),
                        Summary = textBox3.Text,
                        Link = textBox6.Text,
                        Custom = true
                    };
                    Type t = currentData.GetType();
                    var prop = t.GetProperty(museum.Region + "M");
                    object gMuseumData = prop.GetValue(currentData);
                    var props = gMuseumData.GetType().GetProperties();
                    List<Museum> final = (List<Museum>)props[0].GetValue(gMuseumData);
                    final.Add(museum);
                    string jsondata = JsonConvert.SerializeObject(currentData, Formatting.Indented);
                    File.WriteAllText(mFilePath, jsondata);
                    GMarkerGoogle marker = new GMarkerGoogle(museum.LatLang, icon);
                    if (museum.Link != null) { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}\nLink: {museum.Link}"; }
                    else { marker.ToolTipText = $"Name: {museum.Name}\nTown: {museum.Town_city}\nType: {museum.Type}"; }
                    marker.ToolTip = new GMapToolTip(marker);
                    marker.ToolTip.Stroke = Pens.Black;
                    CurrentMarkerOverlay.Markers.Add(marker);
                    var r = MessageBox.Show($"Created \"{museum.Name}\", want to go to it?", "Sucess!", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes)
                    { currentMap.Position = museum.LatLang; }
                    this.Hide();
                    
                }
            }
        }
    }
}
