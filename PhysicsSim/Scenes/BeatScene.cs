﻿using Hanu.ElectroLib.Physics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using PhysicsSim.Interactions;
using PhysicsSim.VBOs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsSim.Scenes
{
    public class BeatScene : Scene
    {

        public float freq1,freq2;

        public static float Amplitude;

        public static float Wavelength;

        public static float time = 0f, speed = 400f;

        private ARenderable _line;

        private readonly Dictionary<string, RectangularButton> _buttons;
        private StandardSlider _freq1Slider, _freq2Slider;
        private RenderText freq1Text, freq2Text;
        private static bool _working1 = false;
        public static List<float> timelist1 = new List<float>();
        public static List<float> timelist2 = new List<float>();

        private static bool _working2 = false;
        private string fontName;

        private static float Sooth(float x)
        {
            x = Math.Abs(x);
            double a = 2 - 2 / (1 + Math.Pow(Math.E, -1/Math.Sqrt(x)));
            return (float)(a*a);
        }

        public static List<System.Numerics.Vector2> WaveLine(float Amp,
                                             float speed,
                                             float frequency_1,
                                             float frequency_2,
                                             float windowwidth,
                                             float time = 0,
                                             float delta = 1)
        {
            List<System.Numerics.Vector2> vectorArrays = new List<System.Numerics.Vector2>();
            System.Numerics.Vector2 i = new System.Numerics.Vector2(-windowwidth / 2, 0);
            float x = 0;

            float wavenumber1 = 2 * MathHelper.Pi / speed * frequency_1;
            float angularFrequency1 = 2 * MathHelper.Pi * frequency_1;
            float wavenumber2 = 2 * MathHelper.Pi / speed * frequency_2;
            float angularFrequency2 = 2 * MathHelper.Pi * frequency_2;

            while (vectorArrays.Count == 0 || vectorArrays.Last().X <= windowwidth / 2)
            {
                double temp = 0f;

                for (int n = 0; n< (int)(timelist1.Count / 2); n++)
                {
                    if (x >= (time - timelist1[2*n+1]) * speed && x <= (time - timelist1[2*n]) * speed)
                    {
                        float starttime1 = timelist1[2*n];
                        float para1 = angularFrequency1 * (time - starttime1) - wavenumber1 * x;
                        temp += Amp * Sooth(para1) * Math.Sin(para1);
                        break;
                    }
                }
                if (timelist1.Count % 2 == 1 && x <= (time - timelist1.Last()) * speed)
                {
                    float starttime1 = timelist1.Last();
                    float para1 = angularFrequency1 * (time - starttime1) - wavenumber1 * x;
                    temp += Amp * Sooth(para1) * Math.Sin(para1);
                }

                for (int n = 0; n< (int)(timelist2.Count / 2); n++)
                {
                    if (x >= (time - timelist2[2*n+1]) * speed && x <= (time - timelist2[2*n]) * speed)
                    {
                        float starttime2 = timelist2[2*n];
                        float para2 = angularFrequency2 * (time - starttime2) - wavenumber2 * x;
                        float endtime2 = timelist2[2*n+1];
                        float para2rev = angularFrequency2 * (endtime2-time) + wavenumber2 * x;
                        temp += Amp * Sooth(para2rev) * Sooth(para2) * Math.Sin(para2);
                        break;
                    }
                }
                if (timelist2.Count % 2 == 1 && x <= (time - timelist2.Last()) * speed)
                {
                    float starttime2 = timelist2.Last();
                    float para2 = angularFrequency2 * (time - starttime2) - wavenumber2 * x;
                    
                    temp += Amp * Sooth(para2) * Math.Sin(para2);
                }

                i.X = x - windowwidth/2 ;
                i.Y = (float)temp;
                vectorArrays.Add(i);
                x += delta;
            }
            return vectorArrays;
        }

        public BeatScene(MainWindow window)
            : base(window)
        {
            _buttons = new Dictionary<string, RectangularButton>();
        }

        public override void Dispose()
        {
            _line?.Dispose();
            foreach (var button in _buttons.Values)
            {
                button.Dispose();
            }
            _buttons.Clear();
            GC.SuppressFinalize(this);
        }

        protected override void OnResize(object sender, EventArgs e)
        {
            _buttons["start1"].Area = new RectangleF(_window.Width / 2f - 75f, -_window.Height / 2f + 15f, 60f, 60f);
            _buttons["start2"].Area = new RectangleF(_window.Width / 2f - 145f, -_window.Height / 2f + 15f, 60f, 60f);
        }

        protected override void OnClosed(object sender, EventArgs e)
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public override void Initialize()
        {
            Time = 0f;
            _working1 = false;
            _working2 = false;
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            ///// BUTTONS /////
            _buttons.Add(
                "start1",
                new RectangularButton(
                    new RectangleF(_window.Width / 2f - 75f, -_window.Height / 2f + 15f, 60f, 60f),
                    ARectangularInteraction.DefaultLineWidth,
                    Color4.Gray,
                    Color4.White,
                    _window.ColoredProgram));
            _buttons["start1"].ButtonPressEvent += (o, a) =>
            {
                if (timelist1.Count() == 0)
                {
                    timelist1.Add(time);
                    timelist1.Add(time + 3f);
                }
                if (time > timelist1.Last())
                {
                    timelist1.Add(time);
                    timelist1.Add(time + 3f);
                }
                _buttons["start1"].FillColor = _working1 ? Color4.Red : Color4.Gray;
            };

            _buttons.Add(
                "start2",
                new RectangularButton(
                    new RectangleF(_window.Width / 2f - 145f, -_window.Height / 2f + 15f, 60f, 60f),
                    ARectangularInteraction.DefaultLineWidth,
                    Color4.Gray,
                    Color4.White,
                    _window.ColoredProgram));
            _buttons["start2"].ButtonPressEvent += (o, a) =>
            {
                if(timelist2.Count() == 0)
                {
                    timelist2.Add(time);
                    timelist2.Add(time + 3f);
                }
                if(time > timelist2.Last())
                {
                    timelist2.Add(time);
                    timelist2.Add(time + 3f);
                }
                
                _buttons["start2"].FillColor = _working2 ? Color4.Red : Color4.Gray;
            };
            ///////////////////
            ///
            _freq1Slider = new StandardSlider(400, 50, 20, 0, 5f, Color4.LightBlue, Color4.White, _window.ColoredProgram);
            _freq1Slider.ValueChangedEvent += (o, ev) =>
            {
                freq1 = ev.NewValue;
                freq1Text.Text = $"f1={ev.NewValue:0.000} Hz";
                
            };
            _freq2Slider = new StandardSlider(400, 50, 20, 0, 200, Color4.LightBlue, Color.White, _window.ColoredProgram) { Value = 100f };
            _freq2Slider.ValueChangedEvent += (o, ev) =>
            {
                freq2 = ev.NewValue;
                freq2Text.Text = $"f2={ev.NewValue:000.00} Hz";
                
            };

            freq1Text = new RenderText(25, fontName, "f=0.000 Hz", Color.Transparent, Color.White, _window.TexturedProgram);
            freq2Text = new RenderText(25, fontName, "f=0.000 Hz", Color.Transparent, Color.White, _window.TexturedProgram);
        }

        protected override void OnRenderFrame(object sender, FrameEventArgs e)
        {
            if (!Enabled) return;
            // Clear the used buffer to paint a new frame onto it
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // Declare that we will use this program
            //GL.UseProgram(_window.ColoredProgram);
            // Get projection matrix and make shaders to compute with this matrix
            Matrix4 projection = GetProjection();

            _line?.Render(ref projection);

            foreach (var button in _buttons.Values)
            {
                button.Render(ref projection);
            }

            _window.SwapBuffers();
        }

        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            var pos = MainWindow.ScreenToCoord(e.X, e.Y, _window.Width, _window.Height);
            _buttons["start1"].PressIfInside(pos);
            _buttons["start2"].PressIfInside(pos);
        }

        private Matrix4 GetProjection()
        {
            return Matrix4.CreateOrthographic(_window.Width, _window.Height, -1f, 1f);
        }

        protected override void OnUpdateFrame(object sender, FrameEventArgs e)
        {
            if (!Enabled) return;
            time += (float)e.Time;
            if (timelist2.Count()!=0)
            {
                if (time <= timelist2.Last())
                {
                    _working2 = true;
                }
                else
                {
                    _working2 = false;
                }
                _buttons["start2"].FillColor = _working2 ? Color4.Red : Color4.Gray;
            }

            if (timelist1.Count()!=0)
            {
                if (time <= timelist1.Last())
                {
                    _working1 = true;
                }
                else
                {
                    _working1 = false;
                }
                _buttons["start1"].FillColor = _working1 ? Color4.Red : Color4.Gray;
            }
            _line = new RenderObject(ObjectFactory.Curve(WaveLine(100f, speed, freq1, freq2 , _window.Width, time), Color4.WhiteSmoke), _window.ColoredProgram);
        }

        private System.Numerics.Vector2 ScreenToCoord(int x, int y)
            => new System.Numerics.Vector2(
                x - _window.Width / 2f,
                -y + _window.Height / 2f
                );
    }
}
