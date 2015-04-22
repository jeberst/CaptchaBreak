﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Net;
using System.IO;
using NAudio.Wave;
using NAudio.Dsp;

namespace SpeechRecognition
{
    class Program
    {
        private const string wavFilePath = @"C:\Projects\MS Project\Samples\wavAudioCaptchas";

        private const string filteredFilePath = @"C:\Projects\MS Project\Samples\filteredAudioCaptchas";

        static void Main(string[] args)
        {
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

        private static void Google()
        {
            try
            {

                FileStream fileStream = File.OpenRead("good-morning-google.flac");
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
                stream.Write(BA_AudioFile, 0, BA_AudioFile.Length);
                stream.Close();

                HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
                if (HWR_Response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                    Console.WriteLine(SR_Response.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
    }
}
