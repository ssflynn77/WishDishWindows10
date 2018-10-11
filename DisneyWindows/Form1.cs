using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisneyWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            monthCalendar1.MinDate = DateTime.Now;
               
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            var xxxx = await DisneyLibWin.DisneyRequests.GetTimesAsync("2018-03-05", "12:00");
        }
    }
}
