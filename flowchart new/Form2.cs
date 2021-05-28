using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization;
using System.Collections;

namespace flowchart_new
{
    public partial class Form2 : Form
    {
       

        public Form2()
        {
            
            InitializeComponent();
            
        }

        public class Data
        {
            string name;
            int values;
            public Data(string name, int values)
            {
                this.name = name;
                this.values = values;
            }
            public string Name {
                get { return name; }
                set { name = value; }
            }
            public int Values
            {
                get { return values; }
                set { values = value; }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string[] array = Class1.temparray;
            

            var data = Getdata(array);


            var chart = chart1.ChartAreas[0];
            chart.AxisX.IntervalType = DateTimeIntervalType.Number;

            chart.AxisX.Interval = 1;
            chart.AxisY.Interval = 10;

            chart.AxisX.Minimum = 0;
            chart.AxisY.Minimum = 0;

            chart.AxisX.Maximum = 10;

            chart.AxisX.LabelStyle.Format = "";
            chart.AxisY.LabelStyle.Format = "";
            Series series = new Series("variable") { ChartType = SeriesChartType.Line, BorderWidth = 2, MarkerSize = 5, MarkerStyle = MarkerStyle.Square };
            chart1.Series.Clear();
            chart1.Series.Add("variable");
            chart1.Series["variable"].IsValueShownAsLabel = true;
            chart1.Series["variable"].ChartType = SeriesChartType.Line;
            chart1.Series["variable"].Color = Color.BlueViolet;
            

            /* foreach (var p in data)
             {
                
               chart1.Series["variable"].Points.AddXY(p.Item1, p.Item2);
                chart1.Series.Add(series);
               

            }

            chart1.Update();*/
            /* Dictionary<string, int> chartdata = new Dictionary<string, int>();
             chartdata.Add(p.Item1, p.Item2);

             foreach (KeyValuePair<string, int> exception in chartdata)
                 chart1.Series["variable"].Points.AddXY(exception.Key, exception.Value);*/


            /*foreach (var s in data)
            {

                value.Add(s.Item2);
                string[] name = new string[] { s.Item1 };  
                int[] value = new int[] { s.Item2 };
               series.Points.DataBindXY(name, value);


           }*/

            ArrayList listdatasource = new ArrayList();
            foreach(var p in data)
            {
                listdatasource.Add(new Data(p.Item1, p.Item2));
               /* textBox1.Text += p.Item1;*/
            }
           /* Chart mychart = chart1;*/
            chart1.DataSource = listdatasource;
           
           
            chart1.Series["variable"].XValueMember = "name";
            chart1.Series["variable"].YValueMembers = "values";
            chart1.Series.Add(series);
            chart1.Update();
        }

        private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

        private static Tuple<string, int>[] Getdata(string[] lines)
        {
            return Array.ConvertAll(lines, line =>
            {
                string[] elems = line.Split('=');
                return new Tuple<string, int> (elems[0], int.Parse(elems[1], EnglishCulture)) ;
            });
        }

       
    }  
    
}  
