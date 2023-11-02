﻿using ApexCharts;
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

        private ApexChart<Data>? Chart { get; set; }

        Channels SelectedChannel = Channels.None;

        private float _amp1Channel1;
        private float _time1Channel1;
        private float _time2Channel1;

        AnnotationsYAxis? Amp1Channel1Annotation;

        AnnotationsXAxis? Time1Channel1Annotation;
        AnnotationsXAxis? Time2Channel1Annotation;
        public float RMSChannel1Bounded { get; set; }
        public float RMSChannel1 { get; set; }
        public float RMSChannel2Bounded { get; set; }
        public float RMSChannel2 { get; set; }

        private ApexChartOptions<Data> ChartOptionsChannel1 { get; set; } = new();
        private ApexChartOptions<Data> ChartOptionsChannel2 { get; set; } = new();

        public List<double>? AmpOrderedData { get; set; }
        public List<double>? Amp2OrderedData { get; set; }

        SineWaveDataGenerator generator = new SineWaveDataGenerator();
        public float Amp1Channel1 { get => _amp1Channel1; set { _amp1Channel1 = value; InvokeAsync(() => { StateHasChanged(); }); } }
        public float Time1Channel1
        {
            get => _time1Channel1;
            set
            {
                _time1Channel1 = value;
                if (value > Time2Channel1) Time2Channel1++;
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



        public List<Data> ChartData { get; set; } = new();
        int IndexOfAmp1Channel1;



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
        protected override async Task OnInitializedAsync()
        {
            ChartOptionsChannel1 = ReturnChartOptions("1");
            await base.OnInitializedAsync();
        }

        void SetSelectedChannel(Channels Channel)
        {
            SelectedChannel = Channel;
        }
        public void GetData()
        {

            ChartData = generator.GenerateEMGSignal(2400, 1f);
            Task.Run(async () => await RenderChart());
            StateHasChanged();
        }
        public async Task RenderChart()
        {
            if (Chart is not null)
            {
                await Chart.RenderAsync();
                await Chart.AddYAxisAnnotationAsync(ZeroLineChartOne, true);


                await Chart.UpdateSeriesAsync();
                await InitCursorsXCursors();
                await InitCursorsYCursors();
            }
        }
        public async Task InitCursorsYCursors()
        {
            if (ChartData.Count != 0 && Chart is not null)
            {
                AmpOrderedData = (from v in ChartData orderby v.yValue descending select v.yValue).ToList();


                IndexOfAmp1Channel1 = 0;




                Amp1Channel1 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 5, MidpointRounding.AwayFromZero);



                Amp1Channel1Annotation = NewAnnotationYAxis("Channel1Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 2, MidpointRounding.AwayFromZero), "Amp 1");




                await Chart.AddYAxisAnnotationAsync(Amp1Channel1Annotation, false);



            }
        }
        public async Task InitCursorsXCursors()
        {
            if (ChartData.Count != 0 && Chart is not null)
            {
                int startIndex = 800;
                int endIndex = (ChartData.Count / 4) * 3;

                Time1Channel1Annotation = NewAnnotationXAxis("Channel1Time1", startIndex, "T1");
                Time2Channel1Annotation = NewAnnotationXAxis("Channel1Time2", endIndex, "T2");


                Time1Channel1 = startIndex;
                Time2Channel1 = endIndex;

                await Chart.AddXAxisAnnotationAsync(Time1Channel1Annotation, false);
                await Chart.AddXAxisAnnotationAsync(Time2Channel1Annotation, false);

            }
        }
        public async Task CursorAdjust(int AdjustAmount)
        {
            if (Chart is not null)
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
                    case Channels.Amp1Channel1:
                        await Chart.RemoveAnnotationAsync("Channel1Amp1");
                        IndexOfAmp1Channel1 += AdjustAmount;
                        IndexOfAmp1Channel1 = Math.Clamp(IndexOfAmp1Channel1, 0, ChartData.Count - 1);
                        Amp1Channel1 = (float)Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 5, MidpointRounding.AwayFromZero);
                        Amp1Channel1Annotation = NewAnnotationYAxis("Channel1Amp1", Math.Round(AmpOrderedData[IndexOfAmp1Channel1], 2, MidpointRounding.AwayFromZero), "Amp 1");
                        await Chart.AddYAxisAnnotationAsync(Amp1Channel1Annotation, false);
                        break;
                }
            }
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
    }
}
