using System;
using System.Collections.Generic;
using System.Text;

namespace FaceAPIDemo
{
    public class VerifyResponse
    {
        public bool isIdentical { get; set; }
        public double confidence { get; set; }
    }
}