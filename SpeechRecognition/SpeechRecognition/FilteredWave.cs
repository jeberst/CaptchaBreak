using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Wave;

namespace SpeechRecognition
{
    public class FilteredWave : ISampleProvider
    {
        private ISampleProvider inProvider;

        private BiQuadFilter filter;
        
        public FilteredWave(ISampleProvider inProvider)
        {
            this.inProvider = inProvider;

            this.filter = BiQuadFilter.LowPassFilter(44100, 5500, 1);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = inProvider.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; ++i)
            {
                buffer[offset + i] = filter.Transform(buffer[offset + i]);
            }

            return samplesRead;
        }

        public WaveFormat WaveFormat
        {
            get { return new WaveFormat(44100, 16, 2); }
        }
    }
}
