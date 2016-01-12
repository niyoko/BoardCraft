using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Models
{
    public struct Size
    {
        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public readonly double Width;
        public readonly double Height;
    }
}
