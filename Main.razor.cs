using ApexCharts;
using BlazorTestApp.Shared;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using BlazorStrap.V5;

namespace BlazorTestApp
{
    public partial class Main
    {
        AnnotationsXAxis Annotation;
        private ApexChart<Data>? Chart { get; set; }
        private ApexChart<Data>? Chart2 { get; set; }

        private List<Data> _ChartData = new();
        private List<Data> _ChartData2 = new();
        GraphDisplay SelectedGraphDisplay = GraphDisplay.Both;
        Channels SelectedChannel = Channels.None;

        private float _amp1Channel1;
        private float _amp2Channel1;
        private float _time1Channel1;
        private float _time2Channel1;
        private float _amp1Channel2;
        private float _amp2Channel2;
        private float _time1Channel2;
        private float _time2Channel2;

        bool DebugEnabled = false;
        bool DCOffsetEnabled = true;
        bool StartDisplayAt800 = false;


        private string[] ports = Array.Empty<string>();
        string MentalabVersion = "unknown";
        AnnotationsYAxis Amp1Channel1Annotation;

        AnnotationsXAxis Time1Channel1Annotation;
        AnnotationsXAxis Time2Channel1Annotation;
        AnnotationsYAxis Amp1Channel2Annotation;

        AnnotationsXAxis Time1Channel2Annotation;
        AnnotationsXAxis Time2Channel2Annotation;


        public string Channel1UpperBound
        {
            get => _C1Upper;
            set
            {
                _C1Upper = value;
                RMSChannel1Bounded = CalulateRMS(ChartData, (int)Time1Channel1, (int)Time2Channel1, float.Parse(Channel1UpperBound), float.Parse(Channel1LowerBound));
            }
        }
        public string Channel1LowerBound
        {
            get => _C1Lower;
            set
            {
                _C1Lower = value;
                RMSChannel1Bounded = CalulateRMS(ChartData, (int)Time1Channel1, (int)Time2Channel1, float.Parse(Channel1UpperBound), float.Parse(Channel1LowerBound));
            }
        }
        public string Channel2UpperBound
        {
            get => _C2Upper;
            set
            {
                _C2Upper = value;
                RMSChannel2Bounded = CalulateRMS(ChartData2, (int)Time1Channel2, (int)Time2Channel2, float.Parse(Channel2UpperBound), float.Parse(Channel2LowerBound));
            }
        }
        public string Channel2LowerBound
        {
            get => _C2Lower;
            set
            {
                _C2Lower = value;
                RMSChannel2Bounded = CalulateRMS(ChartData2, (int)Time1Channel2, (int)Time2Channel2, float.Parse(Channel2UpperBound), float.Parse(Channel2LowerBound));
            }
        }
        public void ToggleDebugData()
        {
            DebugEnabled = !DebugEnabled;
            StateHasChanged();
        }
        public float CalulateRMS(List<Data> Data, int startIndex, int endIndex)
        {
            List<double> Squares = new List<double>(); //List to hold New values
            List<double> DCOffset = new List<double>();
            for (int i = (int)startIndex; i < endIndex; i++)
            {
                var newData = Data[i].yValue;
                DCOffset.Add(newData);
            }
            var DCoffsetValue = (float)DCOffset.Average();

            for (int i = (int)startIndex; i < endIndex; i++)
            {
                var newData = Data[i].yValue - DCoffsetValue;
                Squares.Add(Math.Pow(newData, 2)); // Square of the value
            }
            var Avg = Squares.Average(); //mean of all squares 
            var rms = (float)Math.Sqrt(Avg); // Square root of the avg

            return rms;
        }

        public float CalulateRMS(List<Data> Data, int startIndex, int endIndex, float UpperBound, float lowerBound)
        {
            List<double> Squares = new List<double>(); //List to hold New values

            for (int i = (int)startIndex; i < endIndex; i++) // loop that starts at the specified index, and ends when specifed
            {
                var newData = Data[i].yValue;
                if (newData < UpperBound && newData > lowerBound) // check to see whether the value is withing the range specifed 
                {
                    Squares.Add(Math.Pow(newData, 2)); // Square of the value
                }
            }
            var Avg = Squares.Average(); //mean of all squares 

            var rms = (float)Math.Sqrt(Avg); // Square root of the avg

            return rms;
        }
        bool zeroAtStart = true;
        public float RMSChannel1Bounded { get; set; }
        public float RMSChannel1 { get; set; }
        public float RMSChannel2Bounded { get; set; }
        public float RMSChannel2 { get; set; }

        private ApexChartOptions<Data> ChartOptionsChannel1 { get; set; } = new();
        private ApexChartOptions<Data> ChartOptionsChannel2 { get; set; } = new();

        public List<double> AmpOrderedData { get; set; }
        public List<double> Amp2OrderedData { get; set; }

        SineWaveDataGenerator generator = new SineWaveDataGenerator();
        public float Amp1Channel1 { get => _amp1Channel1; set { _amp1Channel1 = value; InvokeAsync(() => { StateHasChanged(); }); } }
        public float Amp1Channel2 { get => _amp1Channel2; set { _amp1Channel2 = value; InvokeAsync(() => { StateHasChanged(); }); } }
        public float Time1Channel1
        {
            get => _time1Channel1;
            set
            {
                _time1Channel1 = value;
                if (value > Time2Channel1) Time2Channel1++;
                //RMSChannel1= CalulateRMS(ChartData, (int)Time1Channel1, (int)Time2Channel1);
                InvokeAsync(() => { StateHasChanged(); });
            }
        }
        public float Time2Channel1
        {
            get => _time2Channel1;
            set
            {
                _time2Channel1 = value;
                if (value < Time1Channel1) Time1Channel1--;

                InvokeAsync(() => { StateHasChanged(); });
            }
        }

        public float Time1Channel2
        {
            get => _time1Channel2;
            set
            {
                _time1Channel2 = value;
                if (value > Time2Channel2) Time2Channel2++;
                //RMSChannel2 = CalulateRMS(ChartData2, (int)Time1Channel2, (int)Time2Channel2);

                InvokeAsync(() => { StateHasChanged(); });
            }
        }
        public float Time2Channel2
        {
            get => _time2Channel2;
            set
            {
                _time2Channel2 = value;
                if (value < Time1Channel2) Time1Channel2--;


                InvokeAsync(() => { StateHasChanged(); });
            }
        }
        public List<Data> ChartData { get => _ChartData; set { _ChartData = value; } }
        public List<Data> ChartData2 { get => _ChartData2; set { _ChartData2 = value; } }
        int IndexOfAmp1Channel1;

        int IndexOfAmp1Channel2;


        AnnotationsYAxis ZeroLineChartOne = new AnnotationsYAxis
        {
            Id = "ZeroLine",
            StrokeDashArray = 0,
            Y = 0,
            BorderColor = "#ffffff",
            Label = new ApexCharts.Label
            {
                Text = " "
            }
        };
        AnnotationsYAxis ZeroLineChartTwo = new AnnotationsYAxis
        {
            Id = "ZeroLine",
            StrokeDashArray = 0,
            Y = 0,
            BorderColor = "#ffffff",
            Label = new ApexCharts.Label
            {
                Text = " "
            }
        };
        private string _C1Upper;
        private string _C1Lower;
        private string _C2Upper;
        private string _C2Lower;
        private int selectedPortIndex = -1;
        protected override async Task OnInitializedAsync()
        {
            ChartOptionsChannel1 = ReturnChartOptions("1");
            ChartOptionsChannel2 = ReturnChartOptions("2");
            await base.OnInitializedAsync();
        }
        string GraphGridClass = "GraphGrid";
        string GraphOneHidden = "";
        string GraphTwoHidden = "";
        void SetSelectedDisplay(GraphDisplay Display)
        {
            SelectedGraphDisplay = Display;
            switch (SelectedGraphDisplay)
            {
                case GraphDisplay.Both:
                    GraphGridClass = "GraphGrid";
                    GraphOneHidden = "";
                    GraphTwoHidden = "";
                    break;
                case GraphDisplay.ChannelOne:
                    GraphGridClass = "GraphGridOne";
                    GraphOneHidden = "";
                    GraphTwoHidden = "Hidden";
                    break;
                case GraphDisplay.ChannelTwo:
                    GraphGridClass = "GraphGridTwo";
                    GraphOneHidden = "Hidden";
                    GraphTwoHidden = "";
                    break;

            }
        }
        void SetSelectedChannel(Channels Channel)
        {
            SelectedChannel = Channel;
        }
        public void GetData()
        {

            ChartData = generator.GenerateEMGSignal(2400, 1f);
            ChartData2 = generator.GenerateEMGSignal(2400, 1f);
            Task.Run(async () => await RenderChart());
            StateHasChanged();
        }
        public async Task RenderChart()
        {
            await Chart.RenderAsync();
            await Chart.AddYAxisAnnotationAsync(ZeroLineChartOne, true);
            await Chart2.RenderAsync();
            await Chart2.AddYAxisAnnotationAsync(ZeroLineChartTwo, true);// sets the X axis 0 line

            await Chart.UpdateSeriesAsync();
            await Chart2.UpdateSeriesAsync();
            await InitCursorsXCursors();
            await InitCursorsYCursors();
        }
        public double returnTimeFromIndex(int index)
        {
            //add option to add 100 to simulate starting when pulse is fired
            var value = index++ * 300d / 2400;
            if (!zeroAtStart) value += 100;
            return value;
        }
        public async Task InitCursorsYCursors()
        {
            if (ChartData.Count != 0)
            {
                AmpOrderedData = (from v in ChartData orderby v.yValue descending select v.yValue).ToList();
                Amp2OrderedData = (from v in ChartData2 orderby v.yValue descending select v.yValue).ToList();

                IndexOfAmp1Channel1 = 0;

                IndexOfAmp1Channel2 = 0;


                Amp1Channel1 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 5, MidpointRounding.AwayFromZero);

                Amp1Channel2 = (float)Math.Round(Amp2OrderedData[IndexOfAmp1Channel2], 5, MidpointRounding.AwayFromZero);

                Amp1Channel1Annotation = NewAnnotationYAxis("Channel1Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 2, MidpointRounding.AwayFromZero), "Amp 1");


                Amp1Channel2Annotation = NewAnnotationYAxis("Channel2Amp1", Math.Round(Amp2OrderedData[IndexOfAmp1Channel2], 5, MidpointRounding.AwayFromZero), "Amp 1");

                await Chart.AddYAxisAnnotationAsync(Amp1Channel1Annotation, false);

                await Chart2.AddYAxisAnnotationAsync(Amp1Channel2Annotation, false);

            }
        }
        public async Task InitCursorsXCursors()
        {
            if (ChartData.Count != 0)
            {
                int startIndex = 800;
                int endIndex = (ChartData.Count / 4) * 3;

                Time1Channel1Annotation = NewAnnotationXAxis("Channel1Time1", startIndex, "T1");
                Time2Channel1Annotation = NewAnnotationXAxis("Channel1Time2", endIndex, "T2");
                Time1Channel2Annotation = NewAnnotationXAxis("Channel2Time1", startIndex, "T1");
                Time2Channel2Annotation = NewAnnotationXAxis("Channel2Time2", endIndex, "T2");

                Time1Channel1 = startIndex;
                Time2Channel1 = endIndex;
                Time1Channel2 = startIndex;
                Time2Channel2 = endIndex;

                await Chart.AddXAxisAnnotationAsync(Time1Channel1Annotation, false);
                await Chart.AddXAxisAnnotationAsync(Time2Channel1Annotation, false);
                await Chart2.AddXAxisAnnotationAsync(Time1Channel2Annotation, false);
                await Chart2.AddXAxisAnnotationAsync(Time2Channel2Annotation, false);

                RMSChannel1 = CalulateRMS(ChartData, (int)Time1Channel1, (int)Time2Channel1);
                RMSChannel2 = CalulateRMS(ChartData2, (int)Time1Channel2, (int)Time2Channel2);
            }
        }
        public async Task CursorAdjust(int AdjustAmount)
        {
            switch (SelectedChannel)
            {
                case Channels.None: break;
                case Channels.Time1Channel1:
                    await Chart.RemoveAnnotationAsync("Channel1Time1");
                    await Chart.RemoveAnnotationAsync("Channel1Time2");

                    Time1Channel1 += AdjustAmount;
                    Time1Channel1 = Math.Clamp(Time1Channel1, 0, ChartData.Count - 1);
                    Time1Channel1Annotation = NewAnnotationXAxis("Channel1Time1", (int)Time1Channel1, "T1");
                    Time2Channel1Annotation = NewAnnotationXAxis("Channel1Time2", (int)Time2Channel1, "T2");

                    await Chart.AddXAxisAnnotationAsync(Time1Channel1Annotation, false);
                    await Chart.AddXAxisAnnotationAsync(Time2Channel1Annotation, false);

                    break;
                case Channels.Time2Channel1:
                    await Chart.RemoveAnnotationAsync("Channel1Time1");
                    await Chart.RemoveAnnotationAsync("Channel1Time2");

                    Time2Channel1 += AdjustAmount;
                    Time2Channel1 = Math.Clamp(Time2Channel1, 0, ChartData.Count - 1);
                    Time1Channel1Annotation = NewAnnotationXAxis("Channel1Time1", (int)Time1Channel1, "T1");
                    Time2Channel1Annotation = NewAnnotationXAxis("Channel1Time2", (int)Time2Channel1, "T2");

                    await Chart.AddXAxisAnnotationAsync(Time1Channel1Annotation, false);
                    await Chart.AddXAxisAnnotationAsync(Time2Channel1Annotation, false);
                    break;
                case Channels.Time1Channel2:
                    await Chart2.RemoveAnnotationAsync("Channel2Time1");
                    await Chart2.RemoveAnnotationAsync("Channel2Time2");

                    Time1Channel2 += AdjustAmount;
                    Time1Channel2 = Math.Clamp(Time1Channel2, 0, ChartData.Count - 1);
                    Time1Channel2Annotation = NewAnnotationXAxis("Channel2Time1", (int)Time1Channel2, "T1");
                    Time2Channel2Annotation = NewAnnotationXAxis("Channel2Time2", (int)Time2Channel2, "T2");

                    await Chart2.AddXAxisAnnotationAsync(Time1Channel2Annotation, false);
                    await Chart2.AddXAxisAnnotationAsync(Time2Channel2Annotation, false);
                    break;
                case Channels.Time2Channel2:
                    await Chart2.RemoveAnnotationAsync("Channel2Time1");
                    await Chart2.RemoveAnnotationAsync("Channel2Time2");

                    Time2Channel2 += AdjustAmount;
                    Time2Channel2 = Math.Clamp(Time2Channel2, 0, ChartData.Count - 1);
                    Time1Channel2Annotation = NewAnnotationXAxis("Channel2Time1", (int)Time1Channel2, "T1");
                    Time2Channel2Annotation = NewAnnotationXAxis("Channel2Time2", (int)Time2Channel2, "T2");

                    await Chart2.AddXAxisAnnotationAsync(Time1Channel2Annotation, false);
                    await Chart2.AddXAxisAnnotationAsync(Time2Channel2Annotation, false);
                    break;
                case Channels.Amp1Channel1:
                    await Chart.RemoveAnnotationAsync("Channel1Amp1");
                    IndexOfAmp1Channel1 += AdjustAmount;
                    IndexOfAmp1Channel1 = Math.Clamp(IndexOfAmp1Channel1, 0, ChartData.Count - 1);
                    Amp1Channel1 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 5, MidpointRounding.AwayFromZero);
                    Amp1Channel1Annotation = NewAnnotationYAxis("Channel1Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 2, MidpointRounding.AwayFromZero), "Amp 1");
                    await Chart.AddYAxisAnnotationAsync(Amp1Channel1Annotation, false);
                    break;
                case Channels.Amp2Channel1:
                    break;
                case Channels.Amp1Channel2:
                    await Chart2.RemoveAnnotationAsync("Channel2Amp1");
                    IndexOfAmp1Channel2 += AdjustAmount;
                    IndexOfAmp1Channel2 = Math.Clamp(IndexOfAmp1Channel2, 0, ChartData2.Count - 1);
                    Amp1Channel2 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel2], 5, MidpointRounding.AwayFromZero);
                    Amp1Channel2Annotation = NewAnnotationYAxis("Channel2Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel2], 2, MidpointRounding.AwayFromZero), "Amp 1");
                    await Chart2.AddYAxisAnnotationAsync(Amp1Channel2Annotation, false);
                    break;
                case Channels.Amp2Channel2:
                    break;

            }
            RMSChannel1 = CalulateRMS(ChartData, (int)Time1Channel1, (int)Time2Channel1);
            RMSChannel2 = CalulateRMS(ChartData2, (int)Time1Channel2, (int)Time2Channel2);

        }
        public async Task RedrawOnViewSwitch()
        {
            await Chart.RemoveAnnotationAsync("Channel1Time1");
            await Chart.RemoveAnnotationAsync("Channel1Time2");



            Time1Channel1Annotation = NewAnnotationXAxis("Channel1Time1", (int)Time1Channel1, "T1");
            Time2Channel1Annotation = NewAnnotationXAxis("Channel1Time2", (int)Time2Channel1, "T2");

            await Chart.AddXAxisAnnotationAsync(Time1Channel1Annotation, false);
            await Chart.AddXAxisAnnotationAsync(Time2Channel1Annotation, false);

            await Chart2.RemoveAnnotationAsync("Channel2Time1");
            await Chart2.RemoveAnnotationAsync("Channel2Time2");

            Time1Channel2Annotation = NewAnnotationXAxis("Channel2Time1", (int)Time1Channel2, "T1");
            Time2Channel2Annotation = NewAnnotationXAxis("Channel2Time2", (int)Time2Channel2, "T2");

            await Chart2.AddXAxisAnnotationAsync(Time1Channel2Annotation, false);
            await Chart2.AddXAxisAnnotationAsync(Time2Channel2Annotation, false);

            await Chart.RemoveAnnotationAsync("Channel1Amp1");


            Amp1Channel1 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 5, MidpointRounding.AwayFromZero);
            Amp1Channel1Annotation = NewAnnotationYAxis("Channel1Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 2, MidpointRounding.AwayFromZero), "Amp 1");
            await Chart.AddYAxisAnnotationAsync(Amp1Channel1Annotation, false);

            await Chart2.RemoveAnnotationAsync("Channel2Amp1");

            Amp1Channel2 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel2], 5, MidpointRounding.AwayFromZero);
            Amp1Channel2Annotation = NewAnnotationYAxis("Channel2Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel2], 2, MidpointRounding.AwayFromZero), "Amp 1");
            await Chart2.AddYAxisAnnotationAsync(Amp1Channel2Annotation, false);
        }
        public ApexChartOptions<Data> ReturnChartOptions(string ID)
        {
            ApexChartOptions<Data> apexChartOptions = new ApexChartOptions<Data>();
            apexChartOptions.Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Zoom = new Zoom { Enabled = false },
                Animations = new Animations { Enabled = false },
                Id = ID
            };
            apexChartOptions.Tooltip = new Tooltip
            {
                Enabled = true,
                Theme = Mode.Dark,
                X = new TooltipX
                {
                    Show = false
                }
            };
            apexChartOptions.Xaxis = new XAxis
            {
                Labels = new XAxisLabels { Show = false },
                Type = XAxisType.Numeric,
                AxisBorder = new AxisBorder { Show = false },
                Tooltip = new AxisTooltip
                {
                    Enabled = false
                },
                AxisTicks = new AxisTicks { Show = false }

            };
            apexChartOptions.Yaxis = new List<YAxis>();
            apexChartOptions.Grid = new ApexCharts.Grid
            {
                Xaxis = new GridXAxis { Lines = new Lines { Show = true } },
                Yaxis = new GridYAxis { Lines = new Lines { Show = false } }
            };
            apexChartOptions.Yaxis.Add(new YAxis
            {
                TickAmount = 2,
                Labels = new YAxisLabels { Style = new AxisLabelStyle { Colors = new ApexCharts.Color("white") } },
                ForceNiceScale = true,
                DecimalsInFloat = 10
            });
            return apexChartOptions;

        }
        public AnnotationsXAxis NewAnnotationXAxis(string ID, int Index, string Label)
        {
            return new AnnotationsXAxis
            {
                Id = ID,
                StrokeDashArray = 0,
                X = Index,
                BorderColor = "#ffffff",
                Label = new ApexCharts.Label
                {
                    Text = Label,
                    Orientation = Orientation.Horizontal,
                    Style = new ApexCharts.Style
                    {
                        Background = "#ffffff"
                    },
                }
            };
        }
        public AnnotationsYAxis NewAnnotationYAxis(String ID, double Index, string Label)
        {
            return new AnnotationsYAxis
            {
                Id = ID,
                StrokeDashArray = 0,
                Y = Index,
                BorderColor = "#ffffff",
                Label = new ApexCharts.Label
                {
                    Text = Label
                }
            };
        }
        public void ExitApp()
        {
            System.Windows.Application.Current.Shutdown();
        }
        public void ExportData()
        {
            var exportlocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            try
            {
                WriteToCsv(ChartData, exportlocation + @"\" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "_EMGDataExportChannel1.csv");
                WriteToCsv(ChartData2, exportlocation + @"\" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "_EMGDataExportChannel2.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        static void WriteToCsv(List<Data> Data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(Data);
            }
        }
        public string GetCurrentSelectedComPort()
        {
            if (selectedPortIndex == -1)
            {
                return "none";
            }
            if (selectedPortIndex >= 0)
            {
                return ports[selectedPortIndex];
            }
            return "unknown";
        }
        BSModal? modal1 = new();
        string modalMsg = "\0\0\0\0\0";
        private void ShowHelpWindow()
        {
            modal1.ShowAsync();
        }
    }
}
