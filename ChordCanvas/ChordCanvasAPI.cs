using Blazor.Extensions.Canvas.Canvas2D;

namespace ChordCanvas
{
    public static class ChordBoxImage
    {
        public enum Layout
        { 
            One,
            Two
        }

        private static Canvas2DContext _ctx;

        private class Pen
        {
            public Pen(Canvas2DContext ctx, string color, double size)
            {
                _ctx = ctx;
                _color = color;
                _size = size;
            }

            private readonly Canvas2DContext _ctx;
            private readonly string _color;
            private readonly double _size;
            public async Task Set()
            {
                if (_ctx is null) return;

                await _ctx.SetStrokeStyleAsync(_color);
                await _ctx.SetLineWidthAsync((float)_size);
                await _ctx.SetLineCapAsync(LineCap.Round);
            }
        }

        private class Font
        {
            public Font(Canvas2DContext ctx, string fname, double size)
            {
                _ctx = ctx;
                _fname = fname;
                _size = size;
            }

            private readonly Canvas2DContext _ctx;
            private readonly string _fname;
            private readonly double _size;
            public async Task Set()
            {
                if (_ctx is null) return;

                await _ctx.SetFontAsync(_size + "px " + _fname);
                await _ctx.SetTextBaselineAsync(TextBaseline.Top);
            }
        }

        private class FontSize
        {
            public double Width { get; set; }
            public double Height { get; set; }
        }

        private class Bar
        {
            public int String { get; set; }
            public int Position { get; set; }
            public int Length { get; set; }
            public Chord.Fingers Finger { get; set; }
        }

        private static class Graphics
        {
            public static async Task DrawLine(Pen pen, double x1, double y1, double x2, double y2)
            {
                if (_ctx is null) return;

                await _ctx.BeginPathAsync();
                await pen.Set();
                await _ctx.MoveToAsync(x1, y1);
                await _ctx.LineToAsync(x2, y2);
                await _ctx.StrokeAsync();
            }

            public static async Task FillRectangle(string color, double x1, double y1, double x2, double y2)
            {
                if (_ctx is null) return;

                await _ctx.BeginPathAsync();
                await _ctx.SetFillStyleAsync(color);
                await _ctx.RectAsync(x1, y1, x2, y2);
                await _ctx.FillAsync();
            }
            public static async Task DrawCircle(Pen pen, double x1, double y1, double diameter)
            {
                if (_ctx is null) return;

                var radius = diameter / 2;
                await _ctx.BeginPathAsync();
                await pen.Set();
                await _ctx.ArcAsync(x1 + radius, y1 + radius, radius, 0, 2 * Math.PI, false);
                await _ctx.StrokeAsync();
            }
            public static async Task FillCircle(string color, double x1, double y1, double diameter)
            {
                if (_ctx is null) return;

                var radius = diameter / 2;
                await _ctx.BeginPathAsync();
                await _ctx.SetFillStyleAsync(color);
                await _ctx.ArcAsync(x1 + radius, y1 + radius, radius, 0, 2 * Math.PI, false);
                await _ctx.FillAsync();
            }

            public static async Task DrawString(string text, Font font, string color, double x, double y, TextAlign align = TextAlign.Center)
            {
                if (_ctx is null) return;

                await font.Set();
                await _ctx.SetTextAlignAsync(align);
                await _ctx.SetFillStyleAsync(color);
                await _ctx.FillTextAsync(text, x, y);    
            }
        }

        private static double _Size;
        private static string _ChordName = "";
        private static List<int> _ChordPositions = new List<int>();
        private static List<Chord.Fingers> _Fingers = Enumerable.Repeat(Chord.Fingers.NoFinger, 6).ToList();
        private static List<string> _StringNames = new List<string> { "E", "A", "D", "G", "B", "e" };

        private static readonly string _FontName = "Arial";
        private static readonly int _FretCount = 5;

        private static double FretWidth;
        private static double LineWidth;
        private static double BoxWidth;
        private static double BoxHeight;

        private static double ImageWidth { get; set; }
        private static double ImageHeight { get; set; } 
        private static double xStart; //upper corner of the chordbox
        private static double yStart;
        private static double NutHeight;

        private static double DotWidth;

        //Different font sizes
        private static double FretFontSize;
        private static double FingerFontSize;
        private static double NameFontSize;
        private static double SuperScriptFontSize;
        private static double GuitarStringFontSize;
        private static double MarkerWidth;

        private static readonly string _ForegroundBrush = "#000";
        private static readonly string _BackgroundBrush = "#FFF";

        private static int BaseFret;

        private static void ParseChord()
        {
            int minFret = int.MaxValue;
            int maxFret = 0;
            BaseFret = 1;

            foreach (var fret in _ChordPositions)
            {
                if (fret == -1)
                    continue;

                maxFret = Math.Max(fret, maxFret);

                if (fret == 0)
                    continue;

                minFret = Math.Min(fret, minFret);
            }

            if (maxFret > 5)
                BaseFret = minFret;
        }

        private static void InitSizes(double size)
        {
            _Size = size;
            FretWidth = 4 * _Size;
            NutHeight = 0.5 * FretWidth;
            LineWidth = Math.Ceiling(_Size * 0.31);
            DotWidth = Math.Ceiling(0.9 * FretWidth);
            MarkerWidth = 0.5 * FretWidth;
            BoxWidth = 5 * FretWidth + 6 * LineWidth;
            BoxHeight = _FretCount * (FretWidth + LineWidth) + LineWidth;

            //Find out font sizes
            double perc = 2;
            FretFontSize = FretWidth / perc;
            FingerFontSize = FretWidth * 0.8;
            GuitarStringFontSize = FretWidth * 0.8;
            NameFontSize = (FretWidth * 2) / perc;
            SuperScriptFontSize = 0.7 * NameFontSize;
            if (_Size == 1)
            {
                NameFontSize += 2;
                FingerFontSize += 2;
                FretFontSize += 2;
                SuperScriptFontSize += 2;
            }

            xStart = FretWidth;
            yStart = Math.Round(0.2 * SuperScriptFontSize + NameFontSize + NutHeight + 1.7 * MarkerWidth) + 25;

            ImageWidth = (BoxWidth + 5 * FretWidth);
            ImageHeight = (BoxHeight + yStart + FretWidth + FretWidth);
        }

        private static async Task DrawChordBox()
        {
            var drawTasks = new List<Task>();

            Pen pen = new(_ctx, _ForegroundBrush, LineWidth);
            double totalFretWidth = FretWidth + LineWidth;

            for (var i = 0; i <= _FretCount; i++)
            {
                double y = yStart + i * totalFretWidth;
                drawTasks.Add(Graphics.DrawLine(pen, xStart, y, xStart + BoxWidth - LineWidth, y));
            }

            for (int i = 0; i < 6; i++)
            {
                var x = xStart + (i * totalFretWidth);
                drawTasks.Add(Graphics.DrawLine(pen, x, yStart, x, yStart + BoxHeight - LineWidth));
            }

            if (BaseFret == 1)
            {
                //Need to draw the nut
                double nutHeight = FretWidth / 2;
                drawTasks.Add(Graphics.FillRectangle(_ForegroundBrush, xStart - LineWidth / 2, yStart - nutHeight, BoxWidth, nutHeight));
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawBars()
        {
            Dictionary<Chord.Fingers, Bar> bars = new Dictionary<Chord.Fingers, Bar>();
            Bar bar = new Bar();

            for (var i = 0; i < 5; i++)
            {
                if (_ChordPositions[i] != -1 && _ChordPositions[i] != 0 
                    && _Fingers[i] != Chord.Fingers.NoFinger && !bars.ContainsKey(_Fingers[i]))
                {
                    bar.String = i;
                    bar.Position = _ChordPositions[i];
                    bar.Length = 0;
                    bar.Finger = _Fingers[i];

                    for (int j = i + 1; j < 6; j++)
                    {
                        if (_Fingers[j] == bar.Finger && _ChordPositions[j] == _ChordPositions[i])
                        {
                            bar.Length = j - i;
                        }
                    }
                    if (bar.Length > 0)
                    {
                        bars[bar.Finger] = bar;
                    }
                }
            }

            var drawTasks = new List<Task>();

            //TODO: figure out why there are two pens here
            Pen pen = new Pen(_ctx, _ForegroundBrush, LineWidth * 3);
            double totalFretWidth = FretWidth + LineWidth;
            foreach (var b in bars)
            {
                bar = b.Value;
                double xstart = xStart + bar.String * totalFretWidth;
                double xend = xstart + bar.Length * totalFretWidth;
                double y = yStart + ((int)bar.Position - BaseFret + 1) * totalFretWidth - (totalFretWidth / 2);
                pen = new Pen(_ctx, _ForegroundBrush, DotWidth / 2);
                drawTasks.Add(Graphics.DrawLine(pen, xstart, y, xend, y));
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawChordPositions()
        {
            var drawTasks = new List<Task>();

            double xpos = xStart + 0.5 * (LineWidth - FretWidth);
            double yoffset = yStart - FretWidth;
            foreach (var absolutePos in _ChordPositions)
            {
                int relativePos = absolutePos - BaseFret + 1;

                if (relativePos > 0)
                {
                    double ypos = relativePos * (FretWidth + LineWidth) + yoffset;
                    drawTasks.Add(Graphics.FillCircle(_ForegroundBrush, xpos, ypos, DotWidth));
                }
                else if (absolutePos == 0)
                {
                    Pen pen = new Pen(_ctx, _ForegroundBrush, LineWidth);
                    double ypos = yStart - FretWidth;
                    var markerXpos = xpos + ((DotWidth - MarkerWidth) / 2);
                    if (BaseFret == 1)
                    {
                        ypos -= NutHeight;
                    }
                    drawTasks.Add(Graphics.DrawCircle(pen, markerXpos, ypos, MarkerWidth));
                }
                else if (absolutePos == -1)
                {
                    Pen pen = new Pen(_ctx, _ForegroundBrush, LineWidth * 1.5);
                    var ypos = yStart - FretWidth;
                    var markerXpos = xpos + ((DotWidth - MarkerWidth) / 2);
                    if (BaseFret == 1)
                    {
                        ypos -= NutHeight;
                    }
                    drawTasks.Add(Graphics.DrawLine(pen, markerXpos, ypos, markerXpos + MarkerWidth, ypos + MarkerWidth));
                    drawTasks.Add(Graphics.DrawLine(pen, markerXpos, ypos + MarkerWidth, markerXpos + MarkerWidth, ypos));
                }

                xpos += FretWidth + LineWidth;
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawChordPositionsAndFingers()
        {
            var drawTasks = new List<Task>();

            double yoffset = yStart - FretWidth;
            Font font = new Font(_ctx, _FontName, FingerFontSize);

            double xpos = xStart - 0.5 * (FretWidth - LineWidth);
            foreach (var pf in _ChordPositions.Zip(_Fingers, (absolutePos, finger) => (absolutePos, finger)))
            {
                int relativePos = pf.absolutePos - BaseFret + 1;

                if (relativePos > 0)
                {
                    double ypos = relativePos * (FretWidth + LineWidth) + yoffset;
                    drawTasks.Add(Graphics.FillCircle(_ForegroundBrush, xpos, ypos, DotWidth));

                    if (pf.finger != Chord.Fingers.NoFinger)
                    {
                        drawTasks.Add(Graphics.DrawString(pf.finger.ToString("d"), font, _BackgroundBrush, xpos + 0.5 * DotWidth, ypos + LineWidth));
                    }
                }
                else if (pf.absolutePos == 0)
                {
                    Pen pen = new Pen(_ctx, _ForegroundBrush, LineWidth);
                    double ypos = yStart - FretWidth;
                    var markerXpos = xpos + ((DotWidth - MarkerWidth) / 2);
                    if (BaseFret == 1)
                        ypos -= NutHeight;

                    drawTasks.Add(Graphics.DrawCircle(pen, markerXpos, ypos, MarkerWidth));

                    if (pf.finger != Chord.Fingers.NoFinger)
                    {
                        drawTasks.Add(Graphics.DrawString(pf.finger.ToString("d"), font, _BackgroundBrush, xpos + 0.5 * DotWidth, ypos + LineWidth));
                    }
                }
                else if (pf.absolutePos == -1)
                {
                    Pen pen = new Pen(_ctx, _ForegroundBrush, LineWidth * 1.5);
                    var ypos = yStart - FretWidth;
                    var markerXpos = xpos + ((DotWidth - MarkerWidth) / 2);
                    if (BaseFret == 1)
                    {
                        ypos -= NutHeight;
                    }
                    drawTasks.Add(Graphics.DrawLine(pen, markerXpos, ypos, markerXpos + MarkerWidth, ypos + MarkerWidth));
                    drawTasks.Add(Graphics.DrawLine(pen, markerXpos, ypos + MarkerWidth, markerXpos + MarkerWidth, ypos));

                    if (pf.finger != Chord.Fingers.NoFinger)
                    {
                        drawTasks.Add(Graphics.DrawString(pf.finger.ToString("d"), font, _BackgroundBrush, xpos + 0.5 * DotWidth, ypos + LineWidth));
                    }
                }

                xpos += FretWidth + LineWidth;
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawFingers()
        {
            var drawTasks = new List<Task>();

            double xpos = xStart;
            double ypos = yStart + BoxHeight + 0.25 * DotWidth;
            Font font = new Font(_ctx, _FontName, FingerFontSize);
            foreach (var finger in _Fingers)
            {
                if (finger != Chord.Fingers.NoFinger)
                {
                    drawTasks.Add(Graphics.DrawString(finger.ToString("d"), font, _ForegroundBrush, xpos, ypos));
                }
                xpos += (FretWidth + LineWidth);
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawStringNames()
        {
            var drawTasks = new List<Task>();

            double xpos = xStart + (0.5 * LineWidth);
            double ypos = yStart + BoxHeight;
            Font font = new Font(_ctx, _FontName, GuitarStringFontSize);
            foreach (string guitarString in _StringNames)
            {
                drawTasks.Add(Graphics.DrawString(guitarString, font, _ForegroundBrush, xpos, ypos));
                xpos += (FretWidth + LineWidth);
            }

            await Task.WhenAll(drawTasks);
        }

        private static async Task DrawChordName()
        {
            Font nameFont = new Font(_ctx, _FontName, NameFontSize);
            Font superFont = new Font(_ctx, _FontName, SuperScriptFontSize);
            string name;
            string supers;
            if (!_ChordName.Contains("_"))
            {
                name = _ChordName;
                supers = "";
            }
            else
            {
                var parts = _ChordName.Split("_");
                name = parts[0];
                supers = parts[1];
            }

            var drawTasks = new List<Task>();

            double xTextStart = xStart + 0.5 * BoxWidth;
            drawTasks.Add(Graphics.DrawString(name, nameFont, _ForegroundBrush, xTextStart, 0));
            if (supers != "")
            {
                drawTasks.Add(Graphics.DrawString(supers, superFont, _ForegroundBrush, xTextStart, 0));
            }

            if (BaseFret > 1)
            {
                Font fretFont = new Font(_ctx, _FontName, FretFontSize);
                double offset = (FretFontSize - FretWidth) / 2;
                drawTasks.Add(Graphics.DrawString(BaseFret + "fr", fretFont, _ForegroundBrush, xStart + BoxWidth + 0.4 * FretWidth, yStart - offset, TextAlign.Left));
            }

            await Task.WhenAll(drawTasks);
        }


        public static async Task CreateImage(Canvas2DContext ctx, Chord chord, Layout layout, double size)
        {
            _ctx = ctx;
            _ChordPositions = chord.FretList.ToList();
            _Fingers = chord.FingeringList.ToList();
            _ChordName = chord.ChordName;

            InitSizes(size);
            ParseChord();

            await Graphics.FillRectangle(_BackgroundBrush, 0, 0, ImageWidth, ImageHeight);

            var drawTasks = new List<Task>();

            switch (layout)
            {
                case Layout.One:
                    {
                        drawTasks.Add(DrawChordBox());
                        drawTasks.Add(DrawBars());
                        drawTasks.Add(DrawChordPositionsAndFingers());
                        drawTasks.Add(DrawChordName());
                        drawTasks.Add(DrawStringNames());
                    }
                    break;

                case Layout.Two:
                    {
                        drawTasks.Add(DrawChordBox());
                        drawTasks.Add(DrawChordPositions());
                        drawTasks.Add(DrawBars());
                        await DrawChordName();
                        drawTasks.Add(DrawFingers());
                    }
                    break;
            }

            await Task.WhenAll(drawTasks);
        }
    }
}