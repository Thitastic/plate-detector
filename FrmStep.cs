using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LicensePlateRecognition_v0
{
    public partial class FrmStep : Form
    {
        List<Image<Gray, Byte>> list = new List<Image<Gray, byte>>();
        public FrmStep(List<Image<Gray, Byte>> _list)
        {
            InitializeComponent();
            this.list = _list;
        }

        private void FrmStep_Load(object sender, EventArgs e)
        {
           if(this.list.Count < 10)
            {
                pic1.Image = this.list[0].ToBitmap();
                pic2.Image = this.list[1].ToBitmap();
                pic3.Image = this.list[2].ToBitmap();
                pic4.Image = this.list[3].ToBitmap();
                pic5.Image = this.list[4].ToBitmap();
                pic6.Image = this.list[5].ToBitmap();
                pic7.Image = this.list[6].ToBitmap();
                pic8.Image = this.list[7].ToBitmap();
                if(this.list.Count == 9) {
                    pic9.Image = this.list[8].ToBitmap();
                }
            }
        }
    }
}
