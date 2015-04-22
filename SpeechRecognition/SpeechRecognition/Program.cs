using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Net;
using System.IO;
using NAudio.Wave;
using NAudio.Dsp;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using CloudSpeech;

namespace SpeechRecognition
{
    class Program
    {
        private const string wavFilePath = @"C:\Projects\MS Project\Samples\wavAudioCaptchas";

        private const string filteredFilePath = @"C:\Projects\MS Project\Samples\filteredAudioCaptchas";

        static void Main(string[] args)
        {
<<<<<<< HEAD
            string filename = "G:\\CaptchaBreak\\Samples\\wavAudioCaptchas\\test.wav";

            string filtername = "22.wav";

            //Google(filename);
            GoogleCloud(filename);
            //MicrosoftSpeech(filename, filtername);

        }

        private static void MicrosoftSpeech(string filename, string filtername)
        {
            string filteredPath = filterSound(filename, filtername);
=======
            //Google();
            NoiseReduceFiles();
            //MicrosoftSpeech();
        }

        private static void NoiseReduceFiles()
        {
            foreach (string originalPath in Directory.EnumerateFiles(wavFilePath))
            {
                string filename = Path.GetFileName(originalPath);
                string filteredPath = Path.Combine(filteredFilePath, filename);

                NoiseReduceFile(originalPath, filteredPath);
            }
        }

        private static void NoiseReduceFile(string originalPath, string filteredPath)
        {
            using (WaveFileReader reader = new WaveFileReader(originalPath))
            {
                ISampleProvider provider = reader.ToSampleProvider();
                ISampleProvider filteredProvider = new FilteredWave(provider);

                using (WaveFileWriter writer = new WaveFileWriter(filteredPath, filteredProvider.WaveFormat))
                {
                    float[] buffer = new float[44100];
                    int read = 0;
                    while ((read = filteredProvider.Read(buffer, 0, 44100)) != 0)
                    {
                        writer.WriteSamples(buffer, 0, read);
                    }
                }
            }
        }
>>>>>>> origin/master

        private static void MicrosoftSpeech()
        {
            SpeechRecognitionEngine engine = new SpeechRecognitionEngine();

            engine.SetInputToWaveFile(Path.Combine(filteredFilePath, "25366.wav"));
            // engine.SetInputToDefaultAudioDevice();

            Choices choices = new Choices();
            choices.Add("1", "2", "3", "4", "5", "6", "7", "8", "9", "0");

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(choices);

            Grammar g = new Grammar(grammarBuilder);
            engine.LoadGrammar(g);

            engine.SpeechRecognized += (sender, evnt) => Console.WriteLine(evnt.Result.Text);
            engine.SpeechRecognitionRejected += (sender, evnt) => Console.WriteLine("Nope!" + string.Join(",", evnt.Result.Alternates.Select(x => x.Text)));

            engine.RecognizeAsync(RecognizeMode.Multiple);

            Console.ReadLine();
        }

        private static string filterSound(string filename, string filtername)
        {
            string originalPath = filename;
            string filteredPath = "G:\\CaptchaBreak\\Samples\\filteredAudioCaptchas\\" + filtername;

            using (WaveFileReader reader = new WaveFileReader(originalPath))
            {
                ISampleProvider provider = reader.ToSampleProvider();
                ISampleProvider filteredProvider = new FilteredWave(provider);

                Console.WriteLine(reader.WaveFormat.SampleRate + " " + reader.WaveFormat.BitsPerSample + " " + reader.WaveFormat.Channels);

                using (WaveFileWriter writer = new WaveFileWriter(filteredPath, filteredProvider.WaveFormat))
                {
                    float[] buffer = new float[44100];
                    int read = 0;
                    while ((read = filteredProvider.Read(buffer, 0, 44100)) != 0)
                    {
                        writer.WriteSamples(buffer, 0, read);
                    }
                }
            }
            return filteredPath;
        }

        private static void Google(string filename)
        {
            try
            {


                FileStream fileStream = File.OpenRead(filename);
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.SetLength(fileStream.Length);
                fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);
                byte[] BA_AudioFile = memoryStream.GetBuffer();
                HttpWebRequest _HWR_SpeechToText = null;
                _HWR_SpeechToText =
                            (HttpWebRequest)HttpWebRequest.Create(
                                "https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key=AIzaSyC7gzoO7E-Gg6yDtn4lhg6wDvD-qQDMXaQ");
                _HWR_SpeechToText.Credentials = CredentialCache.DefaultCredentials;
                _HWR_SpeechToText.Method = "POST";
                _HWR_SpeechToText.ContentType = "audio/x-flac; rate=44100";
                _HWR_SpeechToText.ContentLength = BA_AudioFile.Length;
                Stream stream = _HWR_SpeechToText.GetRequestStream();
                ConvertToFlac(memoryStream, stream);
                //stream.Write(BA_AudioFile, 0, BA_AudioFile.Length);

                HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
              
                    if (HWR_Response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                        Console.WriteLine(SR_Response.ReadToEnd());
                    }
                    HWR_Response.Close();
           
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }

        private static void GoogleCloud(string filename)
        {
            var SpeechToText = new SpeechToText();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var response = SpeechToText.Recognize(stream);
            }

        }

      private static void ConvertToFlac(Stream sourceStream, Stream destinationStream)
        {
            var audioSource = new WAVReader(null, sourceStream);
            try
            {
                if (audioSource.PCM.SampleRate != 16000)
                {
                    throw new InvalidOperationException("Incorrect frequency - WAV file must be at 16 KHz.");
                }
                var buff = new AudioBuffer(audioSource, 0x10000);
                var flakeWriter = new FlakeWriter(null, destinationStream, audioSource.PCM);
                flakeWriter.CompressionLevel = 8;
                while (audioSource.Read(buff, -1) != 0)
                {
                    flakeWriter.Write(buff);
                }
                flakeWriter.Close();
            }
            finally
            {
                audioSource.Close();
            }
        }
    }

    
}
