using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSpeech
{
    public class TextResponse
    {
        public string Utterance { get; set; }
        public double Confidence { get; set; }
    }
}
