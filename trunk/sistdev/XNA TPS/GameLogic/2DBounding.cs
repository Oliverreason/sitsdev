using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sistdev.GameLogic
{
    public class Bounding2D
    {
        private Int32 type, x, z, width, height;

        #region Propertites
        public Int32 Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        public Int32 X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public Int32 Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }
        public Int32 Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }
        public Int32 Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        public Int32 Radius
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }
        #endregion

        public Bounding2D()
        {
            type = 0;
            x = 0;
            z = 0;
            width = 0;
            height = 0;
        }
        public Bounding2D(Int32 _type, Int32 _x, Int32 _z, Int32 _width, Int32 _height)
        {
            type = _type;
            x = _x;
            z = _z;
            width = _width;
            height = _height;
        }
    }
}
