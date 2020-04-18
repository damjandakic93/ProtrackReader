using Protrack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProtrackReader
{
    public partial class VisualizationForm : Form
    {
        private JumpData m_jump;
        private List<PointF> m_points;
        private List<PointF> m_speed;

        private double[] c_gaussian_7 = {0.00063446,
       0.00096405, 0.00143526, 0.00209363, 0.0029923 , 0.00419031,
       0.00574944, 0.00772932, 0.01018108, 0.01313963, 0.01661535,
       0.02058603, 0.02499036, 0.02972414, 0.0346404 , 0.03955427,
       0.0442528 , 0.04850929, 0.05210099, 0.05482818, 0.05653255,
       0.05711236, 0.05653255, 0.05482818, 0.05210099, 0.04850929,
       0.0442528 , 0.03955427, 0.0346404 , 0.02972414, 0.02499036,
       0.02058603, 0.01661535, 0.01313963, 0.01018108, 0.00772932,
       0.00574944, 0.00419031, 0.0029923 , 0.00209363, 0.00143526,
       0.00096405, 0.00063446 };

        private double[] c_gaussian_5 = {0.00088806, 0.00158611, 0.00272177, 0.00448744, 0.00710844,
       0.01081877, 0.01582012, 0.02222644, 0.03000255, 0.03891121,
       0.04848635, 0.0580487 , 0.0667719 , 0.07379436, 0.07835755,
       0.07994048, 0.07835755, 0.07379436, 0.0667719 , 0.0580487 ,
       0.04848635, 0.03891121, 0.03000255, 0.02222644, 0.01582012,
       0.01081877, 0.00710844, 0.00448744, 0.00272177, 0.00158611,
       0.00088806 };

        private double[] c_gaussian_3 = {0.00147945, 0.00380424, 0.00875346, 0.01802341,
       0.03320773, 0.05475029, 0.08077532, 0.106639  , 0.12597909,
       0.133176  , 0.12597909, 0.106639  , 0.08077532, 0.05475029,
       0.03320773, 0.01802341, 0.00875346, 0.00380424, 0.00147945};

        private const int c_smoothingRadius = 21;

        private double m_exit;

        private double m_deploy;

        public VisualizationForm(JumpData jump)
        {
            m_jump = jump;
            List<double> profile = jump.Profile;
            double t = 0;
            List<double> time = new List<double>();
            while (time.Count < profile.Count)
            {
                time.Add(t);
                t += 0.25;
            }

            List<PointF> points = new List<PointF>();
            for (int i = 0; i < time.Count; i++)
            {
                points.Add(new PointF((float)time[i], (float)profile[i]));
            }

            string title = jump.JumpNumber.ToString();

            Text = title;
            m_points = points;
            m_exit = jump.ExitAltitude;
            m_deploy = jump.DeploymentAltitude;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label8.Text = m_jump.FreefallTime.ToString() + "s";
            label9.Text = m_jump.ExitAltitude.ToString() + "m";
            label10.Text = m_jump.DeploymentAltitude.ToString() + "m";
            label11.Text = (m_jump.AverageSpeed * 3.6).ToString() + "km/h";
            label12.Text = (m_jump.MaxSpeed * 3.6).ToString() + "km/h";
            label13.Text = (m_jump.FirstHalfSpeed * 3.6).ToString() + "km/h";
            label14.Text = (m_jump.SecondHalfSpeed * 3.6).ToString() + "km/h";

            var mapper = LiveCharts.Configurations.Mappers.Xy<PointF>()
                .X(value => value.X)
                .Y(value => value.Y);

            LiveCharts.Charting.For<PointF>(mapper, LiveCharts.SeriesOrientation.All);

            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisY.Clear();

            cartesianChart1.AxisX.Add(new LiveCharts.Wpf.Axis());
            cartesianChart1.AxisY.Add(new LiveCharts.Wpf.Axis());
            cartesianChart1.AxisY.Add(new LiveCharts.Wpf.Axis());

            List<PointF> climb = new List<PointF>();
            List<PointF> fall = new List<PointF>();
            List<PointF> canopy = new List<PointF>();

            m_points.Reverse();
            List<double> max = new List<double>();

            foreach (PointF point in m_points)
            {
                max.Add(Math.Max(max.Any() ? max.Last() : 0, point.Y));
            }

            m_points.Reverse();
            max.Reverse();

            for (int i = 0; i < m_points.Count(); i++)
            {
                if (max[i] < m_deploy)
                {
                    canopy.Add(m_points[i]);
                }
                else if (max[i] < m_exit)
                {
                    fall.Add(m_points[i]);
                }
                else
                {
                    climb.Add(m_points[i]);
                }
            }

            fall.Add(canopy.First());
            climb.Add(fall.First());

            float timeOffset = fall.First().X;

            for (int i = 0; i < m_points.Count; i++)
            {
                m_points[i] = new PointF(m_points[i].X - timeOffset, m_points[i].Y);
            }

            for (int i = 0; i < climb.Count; i++)
            {
                climb[i] = new PointF(climb[i].X - timeOffset, climb[i].Y);
            }

            for (int i = 0; i < fall.Count; i++)
            {
                fall[i] = new PointF(fall[i].X - timeOffset, fall[i].Y);
            }

            for (int i = 0; i < canopy.Count; i++)
            {
                canopy[i] = new PointF(canopy[i].X - timeOffset, canopy[i].Y);
            }

            List<PointF> speed = new List<PointF>() { new PointF(m_points[0].X, 0) };

            for (int i = 1; i < m_points.Count; i++)
            {
                speed.Add(new PointF(m_points[i].X, (m_points[i - 1].Y - m_points[i].Y) * 4 * 3.6f));
            }

            m_speed = new List<PointF>();

            for (int i = 0; i < speed.Count; i++)
            {
                float sum = 0;
                for (int j = -c_smoothingRadius; j <= c_smoothingRadius; j++)
                {
                    float t;

                    if (i + j < 0)
                    {
                        t = speed[0].Y;
                    }
                    else if (i + j >= speed.Count)
                    {
                        t = speed[speed.Count - 1].Y;
                    }
                    else
                    {
                        t = speed[i + j].Y;
                    }
                    
                    sum += (float)(t * c_gaussian_7[j + c_smoothingRadius]);
                    
                }
                m_speed.Add(new PointF(speed[i].X, sum));
            }

            System.Windows.Media.Brush gray = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 128, 128, 128));
            System.Windows.Media.Brush red = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 255, 0, 0));
            System.Windows.Media.Brush blue = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 0, 0, 255));
            System.Windows.Media.Brush green = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 255, 0));

            System.Windows.Media.Brush grayS = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128));
            System.Windows.Media.Brush redS = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            System.Windows.Media.Brush blueS = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
            System.Windows.Media.Brush greenS = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));

            ResetAxes();

            cartesianChart1.Series = new LiveCharts.SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new LiveCharts.ChartValues<PointF>(climb),
                    PointGeometrySize = 0,
                    Title = "Altitude",
                    LabelPoint = point => Math.Round(point.Y, 1) + "m",
                    Fill = gray,
                    Stroke = grayS
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new LiveCharts.ChartValues<PointF>(fall),
                    PointGeometrySize = 0,
                    Title = "Altitude",
                    LabelPoint = point => Math.Round(point.Y, 1) + "m",
                    Fill = red,
                    Stroke = redS
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new LiveCharts.ChartValues<PointF>(canopy),
                    PointGeometrySize = 0,
                    Title = "Altitude",
                    LabelPoint = point => Math.Round(point.Y, 1) + "m",
                    Fill = blue,
                    Stroke = blueS
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new LiveCharts.ChartValues<PointF>(m_speed),
                    PointGeometrySize = 0,
                    Title = "Speed",
                    LabelPoint = point => Math.Round(point.Y, 1) + "km/h",
                    Fill = green,
                    Stroke = greenS,
                    ScalesYAt = 1
                }
            };

            //cartesianChart1.Pan = LiveCharts.PanningOptions.Xy;
            //cartesianChart1.Zoom = LiveCharts.ZoomingOptions.Xy;
            cartesianChart1.DisableAnimations = true;
        }

        private void ResetAxes()
        {
            float range = m_speed.Max(p => p.Y) - m_speed.Min(p => p.Y);
            cartesianChart1.AxisY[1].MinValue = m_speed.Min(p => p.Y) - range * 0.01;
            cartesianChart1.AxisY[1].MaxValue = m_speed.Max(p => p.Y) + range * 0.01;

            cartesianChart1.AxisX[0].MinValue = m_points.Min(p => p.X);
            cartesianChart1.AxisX[0].MaxValue = m_points.Max(p => p.X);
            cartesianChart1.AxisY[0].MinValue = m_points.Min(p => p.Y);
            cartesianChart1.AxisY[0].MaxValue = m_points.Max(p => p.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetAxes();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.WriteLine(string.Join(Environment.NewLine, m_points.Select(p => Math.Round(p.Y, 1).ToString()).ToArray()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
