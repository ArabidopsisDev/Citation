using Citation.Controls;
using Citation.Model;
using Citation.Model.Agriculture;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static Citation.Model.Agriculture.FieldExperiment;

namespace Citation.View.Page
{
    public partial class FieldExperimentPage : UserControl
    {
        private FieldExperiment currentExperiment;
        private Field currentField;
        private Block currentBlock;
        private Plot currentPlot;
        private Signboard currentSignboard;
        private Treatment currentTreatment;

        private Point startPoint;
        private Point currentPoint;
        private bool isDrawing;
        private bool isDragging;
        private UIElement selectedElement;
        private double currentZoom = 1.0;
        private Rectangle drawingPreview;
        private ToolType currentTool = ToolType.Pan;

        private Dictionary<UIElement, object> uiElementMap = new Dictionary<UIElement, object>();
        private Dictionary<string, UIElement> fieldUiElements = new Dictionary<string, UIElement>();
        private Dictionary<string, UIElement> blockUiElements = new Dictionary<string, UIElement>();
        private Dictionary<string, UIElement> plotUiElements = new Dictionary<string, UIElement>();
        private Dictionary<string, UIElement> signboardUiElements = new Dictionary<string, UIElement>();

        private enum ToolType
        {
            Select,
            Field,
            Block,
            Plot,
            Signboard,
            Zoom,
            Pan
        }

        public FieldExperimentPage()
        {
            InitializeComponent();
        }

        private void InitializeNewExperiment()
        {
            currentExperiment = new FieldExperiment
            {
                Name = "新实验",
                Description = "",
                DesignType = ExperimentDesignType.RandomizedCompleteBlock,
                Fields = new System.Collections.ObjectModel.ObservableCollection<Field>(),
                Treatments = new System.Collections.ObjectModel.ObservableCollection<Treatment>()
            };

            ExperimentNameTextBox.Text = currentExperiment.Name;
            ExperimentDescTextBox.Text = currentExperiment.Description;
            DesignTypeComboBox.SelectedIndex = (int)currentExperiment.DesignType;

            TreatmentListBox.ItemsSource = currentExperiment.Treatments;
            AddDefaultTreatments();
            ClearCanvas();
        }

        private void AddDefaultTreatments()
        {
            var treatments = new[]
            {
                new Treatment { Name = "对照组", Description = "无处理", Color = Colors.LightGray },
                new Treatment { Name = "低氮处理", Description = "氮肥: 50kg/ha", Color = Colors.LightGreen, Nitrogen = 50 },
                new Treatment { Name = "高氮处理", Description = "氮肥: 100kg/ha", Color = Colors.Green, Nitrogen = 100 },
                new Treatment { Name = "NPK处理", Description = "氮磷钾复合肥", Color = Colors.LightBlue, Nitrogen = 80, Phosphorus = 40, Potassium = 60 }
            };

            foreach (var treatment in treatments)
            {
                currentExperiment.Treatments.Add(treatment);
            }
        }

        private void InitializeGridBackground()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // 绘制网格
                Pen gridPen = new Pen(Brushes.LightGray, 0.5);
                double gridSize = 20;

                for (double x = 0; x < GridBackground.Width; x += gridSize)
                {
                    drawingContext.DrawLine(gridPen, new Point(x, 0), new Point(x, GridBackground.Height));
                }

                for (double y = 0; y < GridBackground.Height; y += gridSize)
                {
                    drawingContext.DrawLine(gridPen, new Point(0, y), new Point(GridBackground.Width, y));
                }

                // 绘制主要网格线
                Pen majorGridPen = new Pen(Brushes.Gray, 1);
                double majorGridSize = 100;

                for (double x = 0; x < GridBackground.Width; x += majorGridSize)
                {
                    drawingContext.DrawLine(majorGridPen, new Point(x, 0), new Point(x, GridBackground.Height));
                }

                for (double y = 0; y < GridBackground.Height; y += majorGridSize)
                {
                    drawingContext.DrawLine(majorGridPen, new Point(0, y), new Point(GridBackground.Width, y));
                }
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)GridBackground.Width, (int)GridBackground.Height,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);

            ImageBrush gridBrush = new ImageBrush(rtb);
            GridBackground.Fill = gridBrush;
        }

        private void ClearCanvas()
        {
            // 清除所有绘制的元素，但保留背景网格
            var elementsToRemove = MainCanvas.Children.Cast<UIElement>()
                .Where(element => element != GridBackground)
                .ToList();

            foreach (UIElement element in elementsToRemove)
            {
                MainCanvas.Children.Remove(element);
            }

            // 清除映射字典
            uiElementMap.Clear();
            fieldUiElements.Clear();
            blockUiElements.Clear();
            plotUiElements.Clear();
            signboardUiElements.Clear();

            selectedElement = null;
            PropertyPanel.Content = null;
            ExperimentPropertiesExpander.IsExpanded = true;
        }

        private void UpdateToolStates()
        {
            // 更新工具按钮状态
            bool hasFields = currentExperiment.Fields.Count > 0;
            BlockToolButton.IsEnabled = hasFields;

            bool hasBlocks = false;
            foreach (Field field in currentExperiment.Fields)
            {
                if (field.Blocks.Count > 0)
                {
                    hasBlocks = true;
                    break;
                }
            }
            PlotToolButton.IsEnabled = hasBlocks;
        }

        private void UpdateStatusBar()
        {
            string toolText = "";
            switch (currentTool)
            {
                case ToolType.Select: toolText = "选择"; break;
                case ToolType.Field: toolText = "绘制田块"; break;
                case ToolType.Block: toolText = "绘制区组"; break;
                case ToolType.Plot: toolText = "绘制小区"; break;
                case ToolType.Signboard: toolText = "添加告示牌"; break;
                case ToolType.Zoom: toolText = "缩放"; break;
                case ToolType.Pan: toolText = "平移"; break;
            }

            SelectedToolText?.Text = $"工具: {toolText}";
            ZoomLevelText?.Text = $"缩放: {(int)(currentZoom * 100)}%";
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point canvasPos = e.GetPosition(MainCanvas);
            startPoint = canvasPos;

            if (currentTool == ToolType.Pan)
            {
                // 平移模式
                MainCanvas.CaptureMouse();
                isDragging = true;
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }
            else if (currentTool == ToolType.Field || currentTool == ToolType.Block || currentTool == ToolType.Plot)
            {
                // 开始绘制
                isDrawing = true;
                MainCanvas.CaptureMouse();

                // 创建绘制预览
                drawingPreview = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection(new double[] { 4, 2 }),
                    Fill = Brushes.Transparent
                };

                Canvas.SetLeft(drawingPreview, startPoint.X);
                Canvas.SetTop(drawingPreview, startPoint.Y);
                MainCanvas.Children.Add(drawingPreview);
            }
            else if (currentTool == ToolType.Select)
            {
                // 选择模式
                var element = FindClickedElement(canvasPos);
                SelectElement(element);
            }
            else if (currentTool == ToolType.Signboard)
            {
                // 添加告示牌
                CreateSignboard(canvasPos.X, canvasPos.Y);
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 更新光标位置状态
            Point canvasPos = e.GetPosition(MainCanvas);
            CursorPositionText.Text = $"X: {(int)canvasPos.X}, Y: {(int)canvasPos.Y}";
            currentPoint = canvasPos;

            if (isDragging && currentTool == ToolType.Pan)
            {
                // 处理平移
                Vector delta = currentPoint - startPoint;

                CanvasScrollViewer.ScrollToHorizontalOffset(CanvasScrollViewer.HorizontalOffset - delta.X);
                CanvasScrollViewer.ScrollToVerticalOffset(CanvasScrollViewer.VerticalOffset - delta.Y);

                startPoint = currentPoint;
            }
            else if (isDrawing && drawingPreview != null)
            {
                // 更新绘制预览
                double x = Math.Min(startPoint.X, currentPoint.X);
                double y = Math.Min(startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                Canvas.SetLeft(drawingPreview, x);
                Canvas.SetTop(drawingPreview, y);
                drawingPreview.Width = width;
                drawingPreview.Height = height;
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                // 结束平移
                MainCanvas.ReleaseMouseCapture();
                isDragging = false;
                Mouse.OverrideCursor = null;
            }
            else if (isDrawing && drawingPreview != null)
            {
                // 结束绘制
                double x = Math.Min(startPoint.X, currentPoint.X);
                double y = Math.Min(startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                if (width > 10 && height > 10) // 最小尺寸检查
                {
                    switch (currentTool)
                    {
                        case ToolType.Field:
                            CreateField(x, y, width, height);
                            break;
                        case ToolType.Block:
                            if (currentField != null)
                                CreateBlock(x, y, width, height);
                            break;
                        case ToolType.Plot:
                            if (currentBlock != null)
                                CreatePlot(x, y, width, height);
                            break;
                    }
                }

                // 移除绘制预览
                MainCanvas.Children.Remove(drawingPreview);
                drawingPreview = null;

                isDrawing = false;
                MainCanvas.ReleaseMouseCapture();
            }
        }

        private UIElement FindClickedElement(Point position)
        {
            // 从最上层的元素开始检查（最后添加的元素在最上面）
            for (int i = MainCanvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement element = MainCanvas.Children[i];

                if (element == GridBackground || element == drawingPreview)
                    continue;

                if (element is FrameworkElement fe && fe.Tag != null)
                {
                    // 检查矩形、文本等元素
                    Rect bounds = new Rect(
                        Canvas.GetLeft(element),
                        Canvas.GetTop(element),
                        fe.ActualWidth,
                        fe.ActualHeight);

                    if (bounds.Contains(position))
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        private void SelectElement(UIElement element)
        {
            // 清除之前的选择
            if (selectedElement != null)
            {
                if (selectedElement is Shape shape)
                {
                    shape.StrokeThickness /= 2;
                }
                else if (selectedElement is TextBlock textBlock)
                {
                    textBlock.FontWeight = FontWeights.Normal;
                }
            }

            // 设置新选择
            selectedElement = element;

            if (selectedElement != null)
            {
                // 高亮选中的元素
                if (selectedElement is Shape shape)
                {
                    shape.StrokeThickness *= 2;
                }
                else if (selectedElement is TextBlock textBlock)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                }

                // 显示属性面板
                if (uiElementMap.TryGetValue(selectedElement, out var dataObject))
                {
                    if (dataObject is Field field)
                    {
                        currentField = field;
                        ShowFieldProperties(field);
                    }
                    else if (dataObject is Block block)
                    {
                        currentBlock = block;
                        ShowBlockProperties(block);
                    }
                    else if (dataObject is Plot plot)
                    {
                        currentPlot = plot;
                        ShowPlotProperties(plot);
                    }
                    else if (dataObject is Signboard signboard)
                    {
                        currentSignboard = signboard;
                        ShowSignboardProperties(signboard);
                    }
                }
            }
            else
            {
                // 显示实验属性
                PropertyPanel.Content = null;
                ExperimentPropertiesExpander.IsExpanded = true;

                currentField = null;
                currentBlock = null;
                currentPlot = null;
                currentSignboard = null;
            }
        }

        private void CreateField(double x, double y, double width, double height)
        {
            Field field = new Field
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Name = $"田块 {currentExperiment.Fields.Count + 1}",
                Area = width * height / 10000 // 假设1像素=1cm，转换为平方米
            };

            currentExperiment.Fields.Add(field);
            DrawField(field);
            UpdateToolStates();

            // 自动选择新创建的田块
            if (fieldUiElements.TryGetValue(field.Id, out var uiElement))
            {
                SelectElement(uiElement);
            }
        }

        private void DrawField(Field field)
        {
            // 绘制田块矩形
            Rectangle rect = new Rectangle
            {
                Width = field.Width,
                Height = field.Height,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(30, 0, 100, 0)),
                Tag = field.Id
            };

            Canvas.SetLeft(rect, field.X);
            Canvas.SetTop(rect, field.Y);
            MainCanvas.Children.Add(rect);

            // 添加标签
            TextBlock label = new TextBlock
            {
                Text = field.Name,
                Background = Brushes.White,
                Padding = new Thickness(2),
                FontSize = 10,
                Tag = field.Id
            };

            Canvas.SetLeft(label, field.X + 5);
            Canvas.SetTop(label, field.Y + 5);
            MainCanvas.Children.Add(label);

            // 保存映射关系
            uiElementMap[rect] = field;
            uiElementMap[label] = field;
            fieldUiElements[field.Id] = rect;
        }

        private void CreateBlock(double x, double y, double width, double height)
        {
            if (currentField == null) return;

            // 确保区块在田块内部
            double blockX = Math.Max(currentField.X, Math.Min(currentField.X + currentField.Width - width, x));
            double blockY = Math.Max(currentField.Y, Math.Min(currentField.Y + currentField.Height - height, y));
            double blockWidth = Math.Min(width, currentField.Width - (blockX - currentField.X));
            double blockHeight = Math.Min(height, currentField.Height - (blockY - currentField.Y));

            Block block = new Block
            {
                X = blockX - currentField.X, // 相对于田块的坐标
                Y = blockY - currentField.Y, // 相对于田块的坐标
                Width = blockWidth,
                Height = blockHeight,
                Name = $"区组 {currentField.Blocks.Count + 1}",
                Number = currentField.Blocks.Count + 1,
                FieldId = currentField.Id
            };

            currentField.Blocks.Add(block);
            DrawBlock(block);
            UpdateToolStates();

            // 自动选择新创建的区组
            if (blockUiElements.TryGetValue(block.Id, out var uiElement))
            {
                SelectElement(uiElement);
            }
        }

        private void DrawBlock(Block block)
        {
            // 找到所属田块
            Field field = currentExperiment.Fields.FirstOrDefault(f => f.Id == block.FieldId);
            if (field == null) return;

            // 计算绝对坐标
            double absX = field.X + block.X;
            double absY = field.Y + block.Y;

            // 绘制区组矩形
            Rectangle rect = new Rectangle
            {
                Width = block.Width,
                Height = block.Height,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 1.5,
                Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 255)),
                Tag = block.Id
            };

            Canvas.SetLeft(rect, absX);
            Canvas.SetTop(rect, absY);
            MainCanvas.Children.Add(rect);

            // 添加标签
            TextBlock label = new TextBlock
            {
                Text = block.Name,
                Background = Brushes.White,
                Padding = new Thickness(2),
                FontSize = 9,
                Tag = block.Id
            };

            Canvas.SetLeft(label, absX + 5);
            Canvas.SetTop(label, absY + 5);
            MainCanvas.Children.Add(label);

            // 保存映射关系
            uiElementMap[rect] = block;
            uiElementMap[label] = block;
            blockUiElements[block.Id] = rect;
        }

        private void CreatePlot(double x, double y, double width, double height)
        {
            if (currentBlock == null) return;

            // 找到所属田块
            Field field = currentExperiment.Fields.FirstOrDefault(f => f.Id == currentBlock.FieldId);
            if (field == null) return;

            // 计算相对于区块的坐标
            double blockAbsX = field.X + currentBlock.X;
            double blockAbsY = field.Y + currentBlock.Y;

            // 确保小区在区块内部
            double plotX = Math.Max(blockAbsX, Math.Min(blockAbsX + currentBlock.Width - width, x));
            double plotY = Math.Max(blockAbsY, Math.Min(blockAbsY + currentBlock.Height - height, y));
            double plotWidth = Math.Min(width, currentBlock.Width - (plotX - blockAbsX));
            double plotHeight = Math.Min(height, currentBlock.Height - (plotY - blockAbsY));

            Plot plot = new Plot
            {
                X = plotX - blockAbsX, // 相对于区块的坐标
                Y = plotY - blockAbsY, // 相对于区块的坐标
                Width = plotWidth,
                Height = plotHeight,
                Name = $"小区 {currentBlock.Plots.Count + 1}",
                Code = $"{currentBlock.Number}-{currentBlock.Plots.Count + 1}",
                BlockId = currentBlock.Id
            };

            currentBlock.Plots.Add(plot);
            DrawPlot(plot);

            // 自动选择新创建的小区
            if (plotUiElements.TryGetValue(plot.Id, out var uiElement))
            {
                SelectElement(uiElement);
            }
        }

        private void DrawPlot(Plot plot)
        {
            // 找到所属区块和田块
            Block block = currentField?.Blocks.FirstOrDefault(b => b.Id == plot.BlockId);
            if (block == null) return;

            Field field = currentExperiment.Fields.FirstOrDefault(f => f.Id == block.FieldId);
            if (field == null) return;

            // 计算绝对坐标
            double absX = field.X + block.X + plot.X;
            double absY = field.Y + block.Y + plot.Y;

            // 获取处理颜色
            Color plotColor = Colors.LightGray;
            if (!string.IsNullOrEmpty(plot.TreatmentId))
            {
                Treatment treatment = currentExperiment.Treatments.FirstOrDefault(t => t.Id == plot.TreatmentId);
                if (treatment != null)
                {
                    plotColor = treatment.Color;
                }
            }

            // 绘制小区矩形
            Rectangle rect = new Rectangle
            {
                Width = plot.Width,
                Height = plot.Height,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(100, plotColor.R, plotColor.G, plotColor.B)),
                Tag = plot.Id
            };

            Canvas.SetLeft(rect, absX);
            Canvas.SetTop(rect, absY);
            MainCanvas.Children.Add(rect);

            // 添加标签
            TextBlock label = new TextBlock
            {
                Text = plot.Code,
                Background = Brushes.White,
                Padding = new Thickness(1),
                FontSize = 8,
                Tag = plot.Id
            };

            Canvas.SetLeft(label, absX + 2);
            Canvas.SetTop(label, absY + 2);
            MainCanvas.Children.Add(label);

            // 保存映射关系
            uiElementMap[rect] = plot;
            uiElementMap[label] = plot;
            plotUiElements[plot.Id] = rect;
        }

        private void CreateSignboard(double x, double y)
        {
            Signboard signboard = new Signboard
            {
                X = x,
                Y = y,
                Content = "新告示牌",
                FieldId = currentField?.Id
            };

            // 如果当前有选中的田块，则关联到该田块
            if (currentField != null)
            {
                signboard.FieldId = currentField.Id;
            }

            // 添加到实验（需要扩展FieldExperiment类以包含Signboards集合）
            // 这里假设我们在FieldExperiment中添加了Signboards属性
            if (currentExperiment.Signboards == null)
            {
                currentExperiment.Signboards = new System.Collections.ObjectModel.ObservableCollection<Signboard>();
            }
            currentExperiment.Signboards.Add(signboard);

            DrawSignboard(signboard);

            // 自动选择新创建的告示牌
            if (signboardUiElements.TryGetValue(signboard.Id, out var uiElement))
            {
                SelectElement(uiElement);
            }
        }

        private void DrawSignboard(Signboard signboard)
        {
            // 绘制告示牌图标
            Ellipse icon = new Ellipse
            {
                Width = 16,
                Height = 16,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = signboard.Id
            };

            Canvas.SetLeft(icon, signboard.X - 8);
            Canvas.SetTop(icon, signboard.Y - 8);
            MainCanvas.Children.Add(icon);

            // 添加标签
            TextBlock label = new TextBlock
            {
                Text = "ℹ", // 信息图标
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White,
                Padding = new Thickness(2),
                Tag = signboard.Id
            };

            Canvas.SetLeft(label, signboard.X - 6);
            Canvas.SetTop(label, signboard.Y - 8);
            MainCanvas.Children.Add(label);

            // 保存映射关系
            uiElementMap[icon] = signboard;
            uiElementMap[label] = signboard;
            signboardUiElements[signboard.Id] = icon;
        }

        private void ShowFieldProperties(Field field)
        {
            StackPanel panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            // 获取样式资源
            Style propertyLabelStyle = this.FindResource("PropertyLabelStyle") as Style;
            Style propertyTextBoxStyle = this.FindResource("PropertyTextBoxStyle") as Style;

            // 名称
            TextBlock nameLabel = new TextBlock { Text = "名称", Style = propertyLabelStyle };
            TextBox nameTextBox = new TextBox { Text = field.Name, Style = propertyTextBoxStyle };
            nameTextBox.TextChanged += (s, e) => { field.Name = nameTextBox.Text; UpdateFieldVisual(field); };

            // 位置和尺寸
            TextBlock positionLabel = new TextBlock { Text = "位置 (X, Y)", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            StackPanel positionPanel = new StackPanel { Orientation = Orientation.Horizontal };
            TextBox xTextBox = new TextBox { Text = field.X.ToString("F1"), Width = 60, Style = propertyTextBoxStyle };
            TextBox yTextBox = new TextBox { Text = field.Y.ToString("F1"), Width = 60, Style = propertyTextBoxStyle, Margin = new Thickness(5, 2, 0, 0) };
            xTextBox.LostFocus += (s, e) => { if (double.TryParse(xTextBox.Text, out double x)) { field.X = x; UpdateFieldVisual(field); } };
            yTextBox.LostFocus += (s, e) => { if (double.TryParse(yTextBox.Text, out double y)) { field.Y = y; UpdateFieldVisual(field); } };
            positionPanel.Children.Add(xTextBox);
            positionPanel.Children.Add(yTextBox);

            TextBlock sizeLabel = new TextBlock { Text = "尺寸 (宽, 高)", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            StackPanel sizePanel = new StackPanel { Orientation = Orientation.Horizontal };
            TextBox widthTextBox = new TextBox { Text = field.Width.ToString("F1"), Width = 60, Style = propertyTextBoxStyle };
            TextBox heightTextBox = new TextBox { Text = field.Height.ToString("F1"), Width = 60, Style = propertyTextBoxStyle, Margin = new Thickness(5, 2, 0, 0) };
            widthTextBox.LostFocus += (s, e) => { if (double.TryParse(widthTextBox.Text, out double w)) { field.Width = w; UpdateFieldVisual(field); } };
            heightTextBox.LostFocus += (s, e) => { if (double.TryParse(heightTextBox.Text, out double h)) { field.Height = h; UpdateFieldVisual(field); } };
            sizePanel.Children.Add(widthTextBox);
            sizePanel.Children.Add(heightTextBox);

            // 土壤类型
            TextBlock soilLabel = new TextBlock { Text = "土壤类型", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            TextBox soilTextBox = new TextBox { Text = field.SoilType, Style = propertyTextBoxStyle };
            soilTextBox.TextChanged += (s, e) => { field.SoilType = soilTextBox.Text; };

            // 备注
            TextBlock notesLabel = new TextBlock { Text = "备注", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            TextBox notesTextBox = new TextBox { Text = field.Notes, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 60, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Style = propertyTextBoxStyle };
            notesTextBox.TextChanged += (s, e) => { field.Notes = notesTextBox.Text; };

            // 删除按钮
            Button deleteButton = new Button { Content = "删除田块", Margin = new Thickness(0, 20, 0, 0), Padding = new Thickness(4) };
            deleteButton.Click += (s, e) => { DeleteField(field); };

            panel.Children.Add(nameLabel);
            panel.Children.Add(nameTextBox);
            panel.Children.Add(positionLabel);
            panel.Children.Add(positionPanel);
            panel.Children.Add(sizeLabel);
            panel.Children.Add(sizePanel);
            panel.Children.Add(soilLabel);
            panel.Children.Add(soilTextBox);
            panel.Children.Add(notesLabel);
            panel.Children.Add(notesTextBox);
            panel.Children.Add(deleteButton);

            PropertyPanel.Content = panel;
            ExperimentPropertiesExpander.IsExpanded = false;
        }

        private void ShowBlockProperties(Block block)
        {
            StackPanel panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            // 获取样式资源
            Style propertyLabelStyle = this.FindResource("PropertyLabelStyle") as Style;
            Style propertyTextBoxStyle = this.FindResource("PropertyTextBoxStyle") as Style;

            // 名称
            TextBlock nameLabel = new TextBlock { Text = "名称", Style = propertyLabelStyle };
            TextBox nameTextBox = new TextBox { Text = block.Name, Style = propertyTextBoxStyle };
            nameTextBox.TextChanged += (s, e) => { block.Name = nameTextBox.Text; UpdateBlockVisual(block); };

            // 编号
            TextBlock numberLabel = new TextBlock { Text = "编号", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            TextBox numberTextBox = new TextBox { Text = block.Number.ToString(), Style = propertyTextBoxStyle };
            numberTextBox.LostFocus += (s, e) => { if (int.TryParse(numberTextBox.Text, out int num)) { block.Number = num; UpdateBlockVisual(block); } };

            // 删除按钮
            Button deleteButton = new Button { Content = "删除区组", Margin = new Thickness(0, 20, 0, 0), Padding = new Thickness(4) };
            deleteButton.Click += (s, e) => { DeleteBlock(block); };

            panel.Children.Add(nameLabel);
            panel.Children.Add(nameTextBox);
            panel.Children.Add(numberLabel);
            panel.Children.Add(numberTextBox);
            panel.Children.Add(deleteButton);

            PropertyPanel.Content = panel;
            ExperimentPropertiesExpander.IsExpanded = false;
        }

        private void ShowPlotProperties(Plot plot)
        {
            StackPanel panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            // 获取样式资源
            Style propertyLabelStyle = this.FindResource("PropertyLabelStyle") as Style;
            Style propertyTextBoxStyle = this.FindResource("PropertyTextBoxStyle") as Style;

            // 名称
            TextBlock nameLabel = new TextBlock { Text = "名称", Style = propertyLabelStyle };
            TextBox nameTextBox = new TextBox { Text = plot.Name, Style = propertyTextBoxStyle };
            nameTextBox.TextChanged += (s, e) => { plot.Name = nameTextBox.Text; UpdatePlotVisual(plot); };

            // 编号
            TextBlock codeLabel = new TextBlock { Text = "编号", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            TextBox codeTextBox = new TextBox { Text = plot.Code, Style = propertyTextBoxStyle };
            codeTextBox.TextChanged += (s, e) => { plot.Code = codeTextBox.Text; UpdatePlotVisual(plot); };

            // 处理分配
            TextBlock treatmentLabel = new TextBlock { Text = "处理", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            ComboBox treatmentComboBox = new ComboBox { ItemsSource = currentExperiment.Treatments, DisplayMemberPath = "Name", SelectedValuePath = "Id" };
            treatmentComboBox.SelectedValue = plot.TreatmentId;
            treatmentComboBox.SelectionChanged += (s, e) =>
            {
                plot.TreatmentId = treatmentComboBox.SelectedValue?.ToString();
                UpdatePlotVisual(plot);
            };

            // 删除按钮
            Button deleteButton = new Button { Content = "删除小区", Margin = new Thickness(0, 20, 0, 0), Padding = new Thickness(4) };
            deleteButton.Click += (s, e) => { DeletePlot(plot); };

            panel.Children.Add(nameLabel);
            panel.Children.Add(nameTextBox);
            panel.Children.Add(codeLabel);
            panel.Children.Add(codeTextBox);
            panel.Children.Add(treatmentLabel);
            panel.Children.Add(treatmentComboBox);
            panel.Children.Add(deleteButton);

            PropertyPanel.Content = panel;
            ExperimentPropertiesExpander.IsExpanded = false;
        }

        private void ShowSignboardProperties(Signboard signboard)
        {
            StackPanel panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            // 获取样式资源
            Style propertyLabelStyle = this.FindResource("PropertyLabelStyle") as Style;
            Style propertyTextBoxStyle = this.FindResource("PropertyTextBoxStyle") as Style;

            // 内容
            TextBlock contentLabel = new TextBlock { Text = "内容", Style = propertyLabelStyle };
            TextBox contentTextBox = new TextBox { Text = signboard.Content, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 60, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Style = propertyTextBoxStyle };
            contentTextBox.TextChanged += (s, e) => { signboard.Content = contentTextBox.Text; UpdateSignboardVisual(signboard); };

            // 位置
            TextBlock positionLabel = new TextBlock { Text = "位置 (X, Y)", Style = propertyLabelStyle, Margin = new Thickness(0, 10, 0, 0) };
            StackPanel positionPanel = new StackPanel { Orientation = Orientation.Horizontal };
            TextBox xTextBox = new TextBox { Text = signboard.X.ToString("F1"), Width = 60, Style = propertyTextBoxStyle };
            TextBox yTextBox = new TextBox { Text = signboard.Y.ToString("F1"), Width = 60, Style = propertyTextBoxStyle, Margin = new Thickness(5, 2, 0, 0) };
            xTextBox.LostFocus += (s, e) => { if (double.TryParse(xTextBox.Text, out double x)) { signboard.X = x; UpdateSignboardVisual(signboard); } };
            yTextBox.LostFocus += (s, e) => { if (double.TryParse(yTextBox.Text, out double y)) { signboard.Y = y; UpdateSignboardVisual(signboard); } };
            positionPanel.Children.Add(xTextBox);
            positionPanel.Children.Add(yTextBox);

            // 删除按钮
            Button deleteButton = new Button { Content = "删除告示牌", Margin = new Thickness(0, 20, 0, 0), Padding = new Thickness(4) };
            deleteButton.Click += (s, e) => { DeleteSignboard(signboard); };

            panel.Children.Add(contentLabel);
            panel.Children.Add(contentTextBox);
            panel.Children.Add(positionLabel);
            panel.Children.Add(positionPanel);
            panel.Children.Add(deleteButton);

            PropertyPanel.Content = panel;
            ExperimentPropertiesExpander.IsExpanded = false;
        }

        private void UpdateFieldVisual(Field field)
        {
            // 更新田块的视觉表示
            if (fieldUiElements.TryGetValue(field.Id, out var fieldRect))
            {
                if (fieldRect is Rectangle rect)
                {
                    Canvas.SetLeft(rect, field.X);
                    Canvas.SetTop(rect, field.Y);
                    rect.Width = field.Width;
                    rect.Height = field.Height;
                }

                // 更新标签
                foreach (UIElement element in MainCanvas.Children)
                {
                    if (element is TextBlock textBlock && textBlock.Tag as string == field.Id)
                    {
                        Canvas.SetLeft(textBlock, field.X + 5);
                        Canvas.SetTop(textBlock, field.Y + 5);
                        textBlock.Text = field.Name;
                        break;
                    }
                }

                // 更新所有相关的区块和小区
                foreach (var block in field.Blocks)
                {
                    UpdateBlockVisual(block);
                }
            }
        }

        private void UpdateBlockVisual(Block block)
        {
            // 找到所属田块
            Field field = currentExperiment.Fields.FirstOrDefault(f => f.Id == block.FieldId);
            if (field == null) return;

            // 计算绝对坐标
            double absX = field.X + block.X;
            double absY = field.Y + block.Y;

            // 更新区块的视觉表示
            if (blockUiElements.TryGetValue(block.Id, out var blockRect))
            {
                if (blockRect is Rectangle rect)
                {
                    Canvas.SetLeft(rect, absX);
                    Canvas.SetTop(rect, absY);
                    rect.Width = block.Width;
                    rect.Height = block.Height;
                }

                // 更新标签
                foreach (UIElement element in MainCanvas.Children)
                {
                    if (element is TextBlock textBlock && textBlock.Tag as string == block.Id)
                    {
                        Canvas.SetLeft(textBlock, absX + 5);
                        Canvas.SetTop(textBlock, absY + 5);
                        textBlock.Text = block.Name;
                        break;
                    }
                }

                // 更新所有相关的小区
                foreach (var plot in block.Plots)
                {
                    UpdatePlotVisual(plot);
                }
            }
        }

        private void UpdatePlotVisual(Plot plot)
        {
            // 找到所属区块和田块
            Block block = currentField?.Blocks.FirstOrDefault(b => b.Id == plot.BlockId);
            if (block == null) return;

            Field field = currentExperiment.Fields.FirstOrDefault(f => f.Id == block.FieldId);
            if (field == null) return;

            // 计算绝对坐标
            double absX = field.X + block.X + plot.X;
            double absY = field.Y + block.Y + plot.Y;

            // 获取处理颜色
            Color plotColor = Colors.LightGray;
            if (!string.IsNullOrEmpty(plot.TreatmentId))
            {
                Treatment treatment = currentExperiment.Treatments.FirstOrDefault(t => t.Id == plot.TreatmentId);
                if (treatment != null)
                {
                    plotColor = treatment.Color;
                }
            }

            // 更新小区的视觉表示
            if (plotUiElements.TryGetValue(plot.Id, out var plotRect))
            {
                if (plotRect is Rectangle rect)
                {
                    Canvas.SetLeft(rect, absX);
                    Canvas.SetTop(rect, absY);
                    rect.Width = plot.Width;
                    rect.Height = plot.Height;
                    rect.Fill = new SolidColorBrush(Color.FromArgb(100, plotColor.R, plotColor.G, plotColor.B));
                }

                // 更新标签
                foreach (UIElement element in MainCanvas.Children)
                {
                    if (element is TextBlock textBlock && textBlock.Tag as string == plot.Id)
                    {
                        Canvas.SetLeft(textBlock, absX + 2);
                        Canvas.SetTop(textBlock, absY + 2);
                        textBlock.Text = plot.Code;
                        break;
                    }
                }
            }
        }

        private void UpdateSignboardVisual(Signboard signboard)
        {
            // 更新告示牌的视觉表示
            if (signboardUiElements.TryGetValue(signboard.Id, out var signboardIcon))
            {
                if (signboardIcon is Ellipse icon)
                {
                    Canvas.SetLeft(icon, signboard.X - 8);
                    Canvas.SetTop(icon, signboard.Y - 8);
                }

                // 更新标签
                foreach (UIElement element in MainCanvas.Children)
                {
                    if (element is TextBlock textBlock && textBlock.Tag as string == signboard.Id)
                    {
                        Canvas.SetLeft(textBlock, signboard.X - 6);
                        Canvas.SetTop(textBlock, signboard.Y - 8);
                        break;
                    }
                }
            }
        }

        private void DeleteField(Field field)
        {
            // 删除田块及其所有区块和小区
            if (System.Windows.MessageBox.Show($"确定要删除田块 '{field.Name}' 及其所有内容吗？", "确认删除", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                // 删除视觉元素
                if (fieldUiElements.TryGetValue(field.Id, out var fieldRect))
                {
                    MainCanvas.Children.Remove(fieldRect);
                    fieldUiElements.Remove(field.Id);
                    uiElementMap.Remove(fieldRect);
                }

                // 删除标签
                var textBlocksToRemove = MainCanvas.Children
                    .Cast<UIElement>()
                    .Where(element => element is TextBlock textBlock && textBlock.Tag as string == field.Id)
                    .ToList();

                foreach (UIElement element in textBlocksToRemove)
                {
                    MainCanvas.Children.Remove(element);
                    uiElementMap.Remove(element);
                }

                // 删除所有区块和小区
                foreach (var block in field.Blocks.ToList())
                {
                    DeleteBlock(block, false);
                }

                // 从数据模型中删除
                currentExperiment.Fields.Remove(field);

                // 清除选择
                SelectElement(null);
                UpdateToolStates();
            }
        }

        private void DeleteBlock(Block block, bool showConfirmation = true)
        {
            if (showConfirmation && System.Windows.MessageBox.Show($"确定要区块 '{block.Name}' 及其所有小区吗？", "确认删除", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            // 删除视觉元素
            if (blockUiElements.TryGetValue(block.Id, out var blockRect))
            {
                MainCanvas.Children.Remove(blockRect);
                blockUiElements.Remove(block.Id);
                uiElementMap.Remove(blockRect);
            }

            // 删除标签
            var textBlocksToRemove = MainCanvas.Children
                .Cast<UIElement>()
                .Where(element => element is TextBlock textBlock && textBlock.Tag as string == block.Id)
                .ToList();

            foreach (UIElement element in textBlocksToRemove)
            {
                MainCanvas.Children.Remove(element);
                uiElementMap.Remove(element);
            }

            // 删除所有小区
            foreach (var plot in block.Plots.ToList())
            {
                DeletePlot(plot, false);
            }

            // 从数据模型中删除
            var field = currentExperiment.Fields.FirstOrDefault(f => f.Id == block.FieldId);
            if (field != null)
            {
                field.Blocks.Remove(block);
            }

            // 清除选择
            SelectElement(null);
            UpdateToolStates();
        }

        private void DeletePlot(Plot plot, bool showConfirmation = true)
        {
            if (showConfirmation && System.Windows.MessageBox.Show($"确定要删除小区 '{plot.Code}' 吗？", "确认删除", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            // 删除视觉元素
            if (plotUiElements.TryGetValue(plot.Id, out var plotRect))
            {
                MainCanvas.Children.Remove(plotRect);
                plotUiElements.Remove(plot.Id);
                uiElementMap.Remove(plotRect);
            }

            // 删除标签
            var textBlocksToRemove = MainCanvas.Children
                .Cast<UIElement>()
                .Where(element => element is TextBlock textBlock && textBlock.Tag as string == plot.Id)
                .ToList();

            foreach (UIElement element in textBlocksToRemove)
            {
                MainCanvas.Children.Remove(element);
                uiElementMap.Remove(element);
            }

            // 从数据模型中删除
            var block = currentField?.Blocks.FirstOrDefault(b => b.Id == plot.BlockId);
            if (block != null)
            {
                block.Plots.Remove(plot);
            }

            // 清除选择
            SelectElement(null);
        }

        private void DeleteSignboard(Signboard signboard)
        {
            if (System.Windows.MessageBox.Show($"确定要删除告示牌吗？", "确认删除", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                // 删除视觉元素
                if (signboardUiElements.TryGetValue(signboard.Id, out var signboardIcon))
                {
                    MainCanvas.Children.Remove(signboardIcon);
                    signboardUiElements.Remove(signboard.Id);
                    uiElementMap.Remove(signboardIcon);
                }

                // 删除标签
                var textBlocksToRemove = MainCanvas.Children
                    .Cast<UIElement>()
                    .Where(element => element is TextBlock textBlock && textBlock.Tag as string == signboard.Id)
                    .ToList();

                foreach (UIElement element in textBlocksToRemove)
                {
                    MainCanvas.Children.Remove(element);
                    uiElementMap.Remove(element);
                }

                // 从数据模型中删除
                if (currentExperiment.Signboards != null)
                {
                    currentExperiment.Signboards.Remove(signboard);
                }

                // 清除选择
                SelectElement(null);
            }
        }

        private void CanvasScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // 缩放处理
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                currentZoom = Math.Max(0.1, Math.Min(5.0, currentZoom * zoomFactor));

                // 更新缩放文本
                ZoomLevelText.Text = $"缩放: {(int)(currentZoom * 100)}%";

                // 应用缩放变换
                MainCanvas.LayoutTransform = new ScaleTransform(currentZoom, currentZoom);

                e.Handled = true;
            }
        }

        private void ToolButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null || !button.IsChecked.HasValue || !button.IsChecked.Value) return;

            if (button == SelectToolButton) currentTool = ToolType.Select;
            else if (button == FieldToolButton) currentTool = ToolType.Field;
            else if (button == BlockToolButton) currentTool = ToolType.Block;
            else if (button == PlotToolButton) currentTool = ToolType.Plot;
            else if (button == SignboardToolButton) currentTool = ToolType.Signboard;
            else if (button == ZoomToolButton) currentTool = ToolType.Zoom;
            else if (button == PanToolButton) currentTool = ToolType.Pan;

            UpdateStatusBar();
        }

        private void AddTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            // 添加新处理
            Treatment treatment = new Treatment
            {
                Name = $"处理 {currentExperiment.Treatments.Count + 1}",
                Description = "新处理",
                Color = GetRandomColor()
            };

            currentExperiment.Treatments.Add(treatment);
            TreatmentListBox.SelectedItem = treatment;

            // 打开编辑对话框
            EditTreatment(treatment);
        }

        private Color GetRandomColor()
        {
            // 生成随机但不太暗的颜色
            Random random = new Random();
            byte r = (byte)(random.Next(100) + 100);
            byte g = (byte)(random.Next(100) + 100);
            byte b = (byte)(random.Next(100) + 100);

            return Color.FromRgb(r, g, b);
        }

        private void EditTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (TreatmentListBox.SelectedItem is Treatment treatment)
            {
                EditTreatment(treatment);
            }
            else
            {
                System.Windows.MessageBox.Show("请先选择一个处理进行编辑。", "提示", System.Windows.MessageBoxButton.OK);
            }
        }

        private void EditTreatment(Treatment treatment)
        {
            // 创建编辑对话框
            Window editWindow = new Window
            {
                Title = "编辑处理",
                Width = 400,
                Height = 300,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            // 名称
            TextBlock nameLabel = new TextBlock { Text = "名称", Margin = new Thickness(0, 0, 0, 5) };
            TextBox nameTextBox = new TextBox { Text = treatment.Name, Margin = new Thickness(0, 0, 0, 10) };

            // 描述
            TextBlock descLabel = new TextBlock { Text = "描述", Margin = new Thickness(0, 0, 0, 5) };
            TextBox descTextBox = new TextBox { Text = treatment.Description, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 60, Margin = new Thickness(0, 0, 0, 10) };

            TextBlock colorLabel = new TextBlock { Text = "颜色", Margin = new Thickness(0, 0, 0, 5) };
            ColorPicker colorPicker = new ColorPicker
            {
                SelectedColor = treatment.Color,
                Margin = new Thickness(0, 0, 0, 10),
                Width = 200,
                Height = 30,
                DisplayColorAndName = true // 显示颜色和名称:cite[5]
            };

            // 颜色预览
            Rectangle colorPreview = new Rectangle { Width = 100, Height = 20, Fill = new SolidColorBrush(treatment.Color), Margin = new Thickness(0, 0, 0, 10) };

            // 更新颜色预览
            colorPicker.SelectedColorChanged += (s, e) =>
            {
                if (colorPicker.SelectedColor.HasValue)
                {
                    treatment.Color = colorPicker.SelectedColor.Value;
                    colorPreview.Fill = new SolidColorBrush(treatment.Color);
                }
            };


            // 按钮面板
            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button okButton = new Button { Content = "确定", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            Button cancelButton = new Button { Content = "取消", Width = 80 };

            okButton.Click += (s, args) =>
            {
                treatment.Name = nameTextBox.Text;
                treatment.Description = descTextBox.Text;
                editWindow.DialogResult = true;
                editWindow.Close();
            };

            cancelButton.Click += (s, args) =>
            {
                editWindow.DialogResult = false;
                editWindow.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            panel.Children.Add(nameLabel);
            panel.Children.Add(nameTextBox);
            panel.Children.Add(descLabel);
            panel.Children.Add(descTextBox);
            panel.Children.Add(colorLabel);
            panel.Children.Add(colorPreview);
            panel.Children.Add(colorPicker);
            panel.Children.Add(buttonPanel);

            editWindow.Content = panel;
            editWindow.ShowDialog();
        }

        private void UpdateAllPlotsWithTreatment(Treatment treatment)
        {
            // 更新所有使用此处理的小区
            foreach (var field in currentExperiment.Fields)
            {
                foreach (var block in field.Blocks)
                {
                    foreach (var plot in block.Plots)
                    {
                        if (plot.TreatmentId == treatment.Id)
                        {
                            UpdatePlotVisual(plot);
                        }
                    }
                }
            }
        }

        private void RemoveTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (TreatmentListBox.SelectedItem is Treatment treatment)
            {
                if (System.Windows.MessageBox.Show($"确定要删除处理 '{treatment.Name}' 吗？", "确认删除", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                {
                    // 检查是否有小区正在使用此处理
                    bool isInUse = false;
                    foreach (var field in currentExperiment.Fields)
                    {
                        foreach (var block in field.Blocks)
                        {
                            if (block.Plots.Any(p => p.TreatmentId == treatment.Id))
                            {
                                isInUse = true;
                                break;
                            }
                        }
                        if (isInUse) break;
                    }

                    if (isInUse)
                    {
                        System.Windows.MessageBox.Show("无法删除此处理，因为有小区正在使用它。", "提示", System.Windows.MessageBoxButton.OK);
                    }
                    else
                    {
                        currentExperiment.Treatments.Remove(treatment);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("请先选择一个处理进行删除。", "提示", System.Windows.MessageBoxButton.OK);
            }
        }

        private void NewExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定要创建新实验吗？当前未保存的更改将会丢失。", "确认", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                InitializeNewExperiment();
            }
        }

        private void SaveExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            // 更新实验属性
            currentExperiment.Name = ExperimentNameTextBox.Text;
            currentExperiment.Description = ExperimentDescTextBox.Text;
            currentExperiment.DesignType = (ExperimentDesignType)DesignTypeComboBox.SelectedIndex;

            // 选择保存位置
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "大田实验文件 (*.fexp)|*.fexp|所有文件 (*.*)|*.*",
                DefaultExt = ".fexp",
                Title = "保存实验"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new ColorConverter() }
                    };

                    string json = JsonSerializer.Serialize(currentExperiment, options);
                    File.WriteAllText(saveDialog.FileName, json);

                    System.Windows.MessageBox.Show("实验保存成功！", "成功", System.Windows.MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"保存失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK);
                }
            }
        }

        private void LoadExperiment(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new ColorConverter() }
            };

            currentExperiment = JsonSerializer.Deserialize<FieldExperiment>(jsonString, options);

            ExperimentNameTextBox.Text = currentExperiment.Name;
            ExperimentDescTextBox.Text = currentExperiment.Description;
            DesignTypeComboBox.SelectedIndex = (int)currentExperiment.DesignType;
            TreatmentListBox.ItemsSource = currentExperiment.Treatments;

            RedrawAllElements();
        }

        private void LoadExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定要加载实验吗？当前未保存的更改将会丢失。", "确认", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            // 选择加载文件
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "大田实验文件 (*.fexp)|*.fexp|所有文件 (*.*)|*.*",
                DefaultExt = ".fexp",
                Title = "加载实验"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    // 从JSON反序列化
                    string json = File.ReadAllText(openDialog.FileName);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new ColorConverter() }
                    };

                    currentExperiment = JsonSerializer.Deserialize<FieldExperiment>(json, options);

                    // 更新UI
                    ExperimentNameTextBox.Text = currentExperiment.Name;
                    ExperimentDescTextBox.Text = currentExperiment.Description;
                    DesignTypeComboBox.SelectedIndex = (int)currentExperiment.DesignType;
                    TreatmentListBox.ItemsSource = currentExperiment.Treatments;

                    // 重新绘制所有元素
                    RedrawAllElements();

                    System.Windows.MessageBox.Show("实验加载成功！", "成功", System.Windows.MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"加载失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK);
                }
            }
        }

        private void RedrawAllElements()
        {
            // 清除画布
            ClearCanvas();

            // 重新绘制所有田块
            foreach (var field in currentExperiment.Fields)
            {
                DrawField(field);

                // 重新绘制所有区块
                foreach (var block in field.Blocks)
                {
                    DrawBlock(block);

                    // 重新绘制所有小区
                    foreach (var plot in block.Plots)
                    {
                        DrawPlot(plot);
                    }
                }
            }

            // 重新绘制所有告示牌
            if (currentExperiment.Signboards != null)
            {
                foreach (var signboard in currentExperiment.Signboards)
                {
                    DrawSignboard(signboard);
                }
            }

            UpdateToolStates();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = Math.Min(5.0, currentZoom * 1.1);
            ApplyZoom();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = Math.Max(0.1, currentZoom / 1.1);
            ApplyZoom();
        }

        private void ZoomFitButton_Click(object sender, RoutedEventArgs e)
        {
            // 计算所有内容的边界
            double minX = 0, minY = 0, maxX = 0, maxY = 0;
            bool first = true;

            foreach (UIElement element in MainCanvas.Children)
            {
                if (element == GridBackground) continue;

                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                double right = left + ((FrameworkElement)element).ActualWidth;
                double bottom = top + ((FrameworkElement)element).ActualHeight;

                if (first)
                {
                    minX = left; minY = top; maxX = right; maxY = bottom;
                    first = false;
                }
                else
                {
                    minX = Math.Min(minX, left);
                    minY = Math.Min(minY, top);
                    maxX = Math.Max(maxX, right);
                    maxY = Math.Max(maxY, bottom);
                }
            }

            // 计算适合的缩放比例
            double contentWidth = maxX - minX;
            double contentHeight = maxY - minY;

            if (contentWidth > 0 && contentHeight > 0)
            {
                double zoomX = CanvasScrollViewer.ViewportWidth / contentWidth;
                double zoomY = CanvasScrollViewer.ViewportHeight / contentHeight;
                currentZoom = Math.Min(zoomX, zoomY) * 0.9; // 留一些边距

                ApplyZoom();

                // 滚动到内容中心
                CanvasScrollViewer.ScrollToHorizontalOffset(minX + contentWidth / 2 - CanvasScrollViewer.ViewportWidth / 2);
                CanvasScrollViewer.ScrollToVerticalOffset(minY + contentHeight / 2 - CanvasScrollViewer.ViewportHeight / 2);
            }
        }

        private void Zoom100Button_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = 1.0;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            // 应用缩放变换
            MainCanvas.LayoutTransform = new ScaleTransform(currentZoom, currentZoom);

            // 更新缩放文本
            ZoomLevelText.Text = $"缩放: {(int)(currentZoom * 100)}%";
        }

        private class ColorConverter : System.Text.Json.Serialization.JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string colorString = reader.GetString();
                return (Color)System.Windows.Media.ColorConverter.ConvertFromString(colorString);
            }

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private void AIButton_Checked(object sender,RoutedEventArgs e)
        {
            PanToolButton.IsChecked = true;
            currentTool = ToolType.Pan;
            ShowAIWindow();
        }

        private void ShowAIWindow()
        {
            AIContainer.Visibility = Visibility.Visible;
            Storyboard showAnimation = (Storyboard)Resources["ShowAIWindowAnimation"];
            showAnimation.Begin(AIWindow);
            SetToolsEnabled(false);
        }

        private void HideAIWindow()
        {
            Storyboard hideAnimation = (Storyboard)Resources["HideAIWindowAnimation"];
            hideAnimation.Completed += (s, e) =>
            {
                AIContainer.Visibility = Visibility.Collapsed;
                AIToolButton.IsChecked = false;
                SetToolsEnabled(true);
            };
            hideAnimation.Begin(AIWindow);
        }

        private void SetToolsEnabled(bool enabled)
        {
            SelectToolButton.IsEnabled = enabled;
            FieldToolButton.IsEnabled = enabled;
            BlockToolButton.IsEnabled = enabled;
            PlotToolButton.IsEnabled = enabled;
            SignboardToolButton.IsEnabled = enabled;
            ZoomToolButton.IsEnabled = enabled;
            PanToolButton.IsEnabled = enabled;
        }
        private void AIDesignControl_CloseRequested(object sender, EventArgs e)
        {
            HideAIWindow();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeNewExperiment();
            InitializeGridBackground();
            UpdateToolStates();
            UpdateStatusBar();
        }

        private void AIDesignControl_ExperimentGenerated(object sender, EventArgs e)
        {
            var aiControl = (AIDesign)sender;
            LoadExperiment(aiControl.ExperimentFile);
        }
    }
}