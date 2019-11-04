using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace History_11
{
    public partial class LoadingScreen : UserControl
    {
        private delegate void SafeCallDelegate(object sender, EventArgs e);

        internal string currProv;
        internal int currMax;
        internal int Num;
        internal int currcount;
        public LoadingScreen()
        {
            InitializeComponent();
        }

        private void LoadingScreen_Load(object sender, EventArgs e)
        {
            Global.ItemUpdated += Global_ItemUpdated;
            Form1.GroupUpdated += Form1_GroupUpdated;
            Global.ChangeBarMax += Global_ChangeBarMax;
            
            Num = 0;
        }

        private void Global_ChangeBarMax(object sender, EventArgs e)
        {
            if (label1.InvokeRequired || metroProgressBar1.InvokeRequired || label2.InvokeRequired)
            {
                var d = new SafeCallDelegate(Global_ChangeBarMax);
                Invoke(d, sender, e);
            }
            else
            {
                var format = (Global.UpdateEventArgs)e;

                currMax = format.max;
                this.metroProgressBar1.Maximum = currMax;
            }
        }

        private void Form1_GroupUpdated(object sender, EventArgs e)
        {
            if (label1.InvokeRequired || metroProgressBar1.InvokeRequired || label2.InvokeRequired)
            {
                var d = new SafeCallDelegate(Form1_GroupUpdated);
                Invoke(d, sender, e);
            }   
            else
            {
                var ItemName = (Global.UpdateEventArgs)e;
                currProv = ItemName.item;
                this.label1.Text = $"Currently working on {currProv}";
                this.metroProgressBar1.Value = 0;
                Num += 1;
               
            }
            
        }

        private void Global_ItemUpdated(object sender, EventArgs e)
        {
            if (label1.InvokeRequired || metroProgressBar1.InvokeRequired || label2.InvokeRequired)
            {
                var d = new SafeCallDelegate(Global_ItemUpdated);
                Invoke(d, sender, e);
            }
            else
            {
                var ItemName = (Global.UpdateEventArgs)e;
                this.label2.Text = $"({metroProgressBar1.Value}/{metroProgressBar1.Maximum}) Finding: {ItemName.item}";
                this.metroProgressBar1.Value += 1;
                if(metroProgressBar1.Value == currMax)
                {
                    this.Hide();
                }
            }
        }

        public void startProgress(string Prov, int max)
        {
            currcount = 0;
            currProv = Prov;
            currMax = max;
            Num = 0;
            this.label1.Text = $"Currently working on {currProv}";
            this.metroProgressBar1.Maximum = currMax;
           
        }
        
    }
}
