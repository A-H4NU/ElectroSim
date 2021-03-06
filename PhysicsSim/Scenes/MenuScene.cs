﻿using OpenTK;
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
    public sealed class MenuScene : Scene
    {
        private readonly List<TexturedButton> _buttons;

        private readonly List<Scene> _scenes;

        private RenderText _creditText;
        
        public MenuScene(MainWindow window) : base(window)
        {
            _buttons = new List<TexturedButton>();
            _scenes = new List<Scene>()
            {
                this,
                new ElectroScene(Window),
                new SWScene(Window),
                new BeatScene(Window),
                new DopplerScene(Window),
            };
        }

        protected override void OnResize(object sender, EventArgs e)
        {
            base.OnResize(sender, e);

            RearrangeButtons();
            _creditText.Position = new Vector3(Window.Width / 2f - _creditText.Width / 2f, -Window.Height / 2f + _creditText.Height / 2f, 0f);
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            Color4 border = new Color4(0x50, 0xB0, 0xB2, 0xFF);
            _buttons.AddRange(new TexturedButton[]
            {
                new TexturedButton(500f, 500f, 5f, border, @"Textures\electro.jpg", Window.ColoredProgram, Window.TexturedProgram),
                new TexturedButton(500f, 500f, 5f, border, @"Textures\standing_wave.jpg", Window.ColoredProgram, Window.TexturedProgram),
                new TexturedButton(500f, 500f, 5f, border, @"Textures\beat_wave.jpg", Window.ColoredProgram, Window.TexturedProgram),
                new TexturedButton(500f, 500f, 5f, border, @"Textures\doppler.jpg", Window.ColoredProgram, Window.TexturedProgram),
            });
            for (int i = 0; i < _buttons.Count; ++i)
            {
                int idx = i;
                _buttons[i].ButtonPressEvent += (o, _) => ActivateScene(idx + 1);
            }
            _creditText = new RenderText(15, "Arial", "by Seungwoo Han & Jaewon Jeon", Color.Transparent, Color.White, Window.TexturedProgram);
            ActivateScene(0);
            GL.Enable(EnableCap.Blend);

            RearrangeButtons();
        }

        private void ActivateScene(int index)
        {
            for (int i = 0; i < _scenes.Count; ++i)
            {
                _scenes[i].Enabled = false;
            }
            _scenes[index].Enabled = true;
            _scenes[index].Initialize();
        }

        private void RearrangeButtons()
        {
            int gridW = 2, gridH = 2;
            float tileside = (float)Window.Width / Window.Height < 1.5f ?
                Window.Width / gridW :
                Window.Height / gridH;
            tileside *= 0.95f;
            for (int i = 0; i < Math.Min(_buttons.Count, gridW * gridH); ++i)
            {
                int j = gridH - i / gridW - 1, k = i % gridW;
                RectangleF rectangle = new RectangleF(
                    0.5f * tileside * gridW * (2f * k / gridW - 1),
                    0.5f * tileside * gridH * (2f * j / gridH - 1),
                    tileside, tileside);
                _buttons[i].Area = rectangle;
            }
        }

        protected override void OnUpdateFrame(object sender, FrameEventArgs e)
        {
            if (Enabled)
            {
            }
        }

        protected override void OnRenderFrame(object sender, FrameEventArgs e)
        {
            if (Enabled)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                Matrix4 projection = MainWindow.GetProjection(Window.Width, Window.Height);
                foreach (var button in _buttons)
                {
                    button.Render(ref projection);
                }

                _creditText.Render(ref projection);

                Window.SwapBuffers();
            }
        }

        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Enabled)
            {
                var pos = MainWindow.ScreenToCoord(e.X, e.Y, Window.Width, Window.Height);
                foreach (var button in _buttons)
                {
                    button.PressIfInside(pos);
                }
            }
        }

        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape && !e.IsRepeat)
            {
                if (_scenes[0].Enabled)
                {
                    Window.Close();
                }
                else
                {
                    ActivateScene(0);
                    RearrangeButtons();
                }
            }
            if (e.Key == Key.F11 && !e.IsRepeat)
            {
                if (Window.WindowState == WindowState.Fullscreen)
                {
                    Window.WindowState = WindowState.Normal;
                }
                else
                {
                    Window.WindowState = WindowState.Fullscreen;
                }
                RearrangeButtons();
            }
            if (e.Key == Key.R && !e.IsRepeat)
            {
                RearrangeButtons();
            }
        }

        protected override void OnClosed(object sender, EventArgs e)
        {
            Dispose();
        }

        public override void Dispose()
        {
            foreach (var button in _buttons)
            {
                button.Dispose();
            }
            _buttons.Clear();
        }

        public override void Initialize()
        {
            
        }
    }
}
