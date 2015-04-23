using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace DigitSpeechSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }

        private void Run()
        {
            string filteredCaptchas = @"C:\Projects\MS Project\Samples\filteredAudioCaptchas\";

            foreach (string filePath in Directory.EnumerateFiles(filteredCaptchas, "*", SearchOption.TopDirectoryOnly))
            {
                Split(filePath);
            }
        }

        private void Split(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileNameWithoutExtension(filePath);

            int count = 11025;
            double minFreq = 0.1;

            List<List<float>> samples = new List<List<float>>();
            samples.Add(new List<float>());


            using (WaveFileReader reader = new WaveFileReader(filePath))
            {
                ISampleProvider readerProvider = reader.ToSampleProvider();

                float[] buffer = new float[44100];
                int read;
                int clearCount = 0;
                bool cleared = true;

                while ((read = readerProvider.Read(buffer, 0, 44100)) != 0)
                {
                    for (int i = 0; i < read; ++i)
                    {
                        if (i % 2 == 1)
                        {
                            if (!cleared) samples[samples.Count - 1].Add(buffer[i]);
                            continue;
                        }

                        if (cleared && buffer[i] < minFreq)
                        {
                            continue;
                        }

                        if (cleared)
                        {
                            cleared = false;
                        }

                        if (clearCount == count)
                        {
                            clearCount = 0;
                            samples.Add(new List<float>());
                            cleared = true;
                        }

                        if (buffer[i] < minFreq)
                        {
                            clearCount++;
                        }
                        else
                        {
                            clearCount = 0;
                        }

                        samples[samples.Count - 1].Add(buffer[i]);
                    }
                }
            }

            for (int i = 0; i < samples.Count; ++i)
            {
                string directoryPath = Path.Combine(directory, filename);
                Directory.CreateDirectory(directoryPath).Attributes &= ~FileAttributes.ReadOnly;

                using (WaveFileWriter writer = new WaveFileWriter(Path.Combine(directoryPath, i + ".wav"), new WaveFormat(44100, 16, 2)))
                {
                    float[] current = samples[i].ToArray();

                    writer.WriteSamples(current, 0, current.Length);
                }
            }
        }
    }
}
