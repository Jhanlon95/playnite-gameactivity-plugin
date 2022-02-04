﻿using CommonPlayniteShared.Converters;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using GameActivity.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GameActivity.Controls
{
    /// <summary>
    /// Logique d'interaction pour GanttControl.xaml
    /// </summary>
    public partial class GanttControl : UserControl
    {
        internal static readonly ILogger logger = LogManager.GetLogger();
        internal static IResourceProvider resources = new ResourceProvider();

        private DataContextGanttControl dataContextGanttControl = new DataContextGanttControl();


        #region Properties
        public DependencyProperty ColorItemProperty;
        public SolidColorBrush ColorItem { get; set; }

        public string DataName
        {
            get { return (string)GetValue(DataNameProperty); }
            set { SetValue(DataNameProperty, value); }
        }

        public static readonly DependencyProperty DataNameProperty = DependencyProperty.Register(
            nameof(DataName),
            typeof(string),
            typeof(GanttControl),
            new FrameworkPropertyMetadata(string.Empty));


        public int ColumnCount
        {
            get { return (int)GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }

        public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.Register(
            nameof(ColumnCount),
            typeof(int),
            typeof(GanttControl),
            new FrameworkPropertyMetadata(10, ControlPropertyChangedCallback));


        public DateTime LastDate
        {
            get { return (DateTime)GetValue(LastDateProperty); }
            set { SetValue(LastDateProperty, value); }
        }

        public static readonly DependencyProperty LastDateProperty = DependencyProperty.Register(
            nameof(LastDate),
            typeof(DateTime),
            typeof(GanttControl),
            new FrameworkPropertyMetadata(DateTime.Now, ControlPropertyChangedCallback));


        public List<GanttValue> Values
        {
            get { return (List<GanttValue>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            nameof(Values),
            typeof(List<GanttValue>),
            typeof(GanttControl),
            new FrameworkPropertyMetadata(new List<GanttValue>(), ControlPropertyChangedCallback));

        public bool OnlyDate
        {
            get { return (bool)GetValue(OnlyDateProperty); }
            set { SetValue(OnlyDateProperty, value); }
        }

        public static readonly DependencyProperty OnlyDateProperty = DependencyProperty.Register(
            nameof(OnlyDate),
            typeof(bool),
            typeof(GanttControl),
            new FrameworkPropertyMetadata(false, ControlPropertyChangedCallback));



        private static void ControlPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is GanttControl obj && e.NewValue != e.OldValue)
            {
                ContentControl contentControl = obj.Parent as ContentControl;
                switch (e.Property.Name)
                {
                    case "ColumnCount":
                        obj.DefineColumn();
                        obj.SetData();
                        obj.SetDataDate();
                        break;

                    case "LastDate":
                    case "Values":
                        obj.SetData();
                        obj.SetDataDate();
                        break;

                    case "OnlyDate":
                        obj.SetDataDate();
                        break;
                }
            }
        }
        #endregion


        public GanttControl()
        {
            InitializeComponent();
            this.DataContext = dataContextGanttControl;

            PART_Gantt.Children.Clear();

            Random rnd = new Random();
            ColorItem = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));

            DefineColumn();
        }


        private void DefineColumn()
        {
            PART_Gantt.ColumnDefinitions.Clear();
            for (int idx = 0; idx <= ColumnCount; idx++)
            {
                try
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                    PART_Gantt.ColumnDefinitions.Add(columnDefinition);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
                }
            }
        }


        private void SetData()
        {           
            if (Values == null || Values.Count == 0)
            {
                return;
            }

            PART_Gantt.Children.Clear();

            for (int idx = ColumnCount; idx >= 0; idx--)
            {
                try
                {
                    DateTime dt = LastDate.AddDays(idx * -1);
                    var finded = Values.Find(x => x.PlayDate.ToString("yyyy-MM-dd") == dt.ToString("yyyy-MM-dd"));

                    if (finded != null)
                    {
                        Border border = new Border();
                        border.Background = ColorItem;
                        Grid.SetColumn(border, ColumnCount - idx);

                        border.BorderThickness = (Thickness)resources.GetResource("CommonToolTipBorderThickness");
                        border.BorderBrush = (Brush)resources.GetResource("CommonToolTipBorderBrush");

                        // Tooltip
                        StackPanel stackPanel = new StackPanel();

                        TextBlock textBlock = new TextBlock();
                        textBlock.FontWeight = FontWeights.Bold;

                        Binding bindingText = new Binding();
                        bindingText.Source = DataName;
                        bindingText.Mode = BindingMode.OneWay;
                        textBlock.SetBinding(TextBlock.TextProperty, bindingText);

                        Binding bindingVisibility = new Binding();
                        bindingVisibility.Converter = new StringNullOrEmptyToVisibilityConverter();
                        bindingVisibility.Source = DataName;
                        bindingVisibility.Mode = BindingMode.OneWay;
                        textBlock.SetBinding(VisibilityProperty, bindingVisibility);

                        stackPanel.Children.Add(textBlock);

                        Label label = new Label();
                        label.Content = new LocalDateConverter().Convert(finded.PlayDate, null, null, CultureInfo.CurrentCulture)
                             + " - " + new PlayTimeToStringConverterWithZero().Convert(finded.PlayTime, null, null, CultureInfo.CurrentCulture);

                        stackPanel.Children.Add(label);

                        border.ToolTip = stackPanel;

                        PART_Gantt.Children.Add(border);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
                }
            }
        }


        private void SetDataDate()
        {
            if (OnlyDate)
            {
                PART_Gantt.Children.Clear();
                LocalDateConverter localDateConverter = new LocalDateConverter();

                for (int idx = ColumnCount; idx >= 0; idx--)
                {
                    try
                    {
                        DateTime dt = LastDate.AddDays(idx * -1);

                        Grid grid = new Grid();
                        Grid.SetColumn(grid, ColumnCount - idx);

                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = localDateConverter.Convert(dt, null, null, CultureInfo.CurrentCulture).ToString();
                        
                        RotateTransform rotateTransform = new RotateTransform { Angle = 270 };
                        textBlock.LayoutTransform = rotateTransform;

                        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                        textBlock.VerticalAlignment = VerticalAlignment.Center;

                        textBlock.TextAlignment = TextAlignment.Center;

                        grid.Children.Add(textBlock);
                        PART_Gantt.Children.Add(grid);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false);
                    }
                }
            }
        }
    }


    public class DataContextGanttControl : ObservableObject
    {

    }
}
