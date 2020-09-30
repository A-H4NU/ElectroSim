﻿using OpenTK;
using OpenTK.Graphics;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroSim.Vertices
{
    public struct ColoredVertex
    {
        public const int SIZE = 32;

        private Vector4 _position;
        private Color4 _color;

        public Vector4 Position { get => _position; }
        public Color4 Color { get => _color; }

        public ColoredVertex(Vector4 position, Color4 color)
        {
            _position = position;
            _color = color;
        }

        public override string ToString()
        {
            return _position.ToString();
        }
    }
}