using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudSpeech;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using NAudio.Dsp;
using NAudio.Wave;

namespace SpeechRecognition
{
    class Program
    {
        private const string wavFilePath = @"C:\Projects\MS Project\Samples\wavAudioCaptchas";

        private const string filteredFilePath = @"C:\Projects\MS Project\Samples\filteredAudioCaptchas";

        static void Main(string[] args)
        {
            string filename = "G:\\CaptchaBreak\\Samples\\wavAudioCaptchas\\12108.wav";

            string filtername = "22.wav";

            //Google(filename);
            //GoogleCloud(filename);
            //MicrosoftSpeech(filename, filtername);
            Apple(filename);
            //AppleFail2(filename);

        }

        private static void MicrosoftSpeech(string filename, string filtername)
        {
            string filteredPath = filterSound(filename, filtername);

            Microsoft m = new Microsoft();

            string result = m.Estimate(filteredPath);
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


        private static void StereoToMono(string filename)
        {

            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                StereoToMonoProvider16 sound = new StereoToMonoProvider16(reader);
                sound.LeftVolume = 1;
                ISampleProvider provider = sound.ToSampleProvider();
                using (WaveFileWriter writer = new WaveFileWriter(filename.Replace(".wav", "mono.wav"), sound.WaveFormat))
                {
                    float[] buffer = new float[44100];
                    int read = 0;
                    while ((read = provider.Read(buffer, 0, 44100)) != 0)
                    {
                        //writer.Write(buffer, 0, read);
                        writer.WriteSamples(buffer, 0, read);
                    }
                }

            }

        }

        private static void MicrosoftSpeech()
        {
            Microsoft m = new Microsoft();

            foreach (string file in Directory.EnumerateFiles(filteredFilePath)) 
            {
                string actual = Path.GetFileNameWithoutExtension(file);
                string result = m.Estimate(Path.Combine(filteredFilePath, file));

                Console.WriteLine(actual + " - " + result);
            }

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

        private static void Apple(string filename)
        {
            try
            {
                StereoToMono(filename);
                FileStream fileStream = File.OpenRead(filename.Replace(".wav", "mono.wav"));
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.SetLength(fileStream.Length);
                fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);
                byte[] BA_AudioFile = memoryStream.GetBuffer();
                HttpWebRequest _HWR_SpeechToText = null;
                _HWR_SpeechToText =
                            (HttpWebRequest)HttpWebRequest.Create(
                                "https://dictation.nuancemobility.net/NMDPAsrCmdServlet/dictation?appId=NMDPTRIAL_j_eberst_knights_ucf_edu20150421210830&appKey=35ba89c3c244e1d1b438654c5f0121e4818267620c8ca34fb67fc55eaad98adf9314a2565884270cbc8c591b8d7aff0ffc667108f31457a9941d073ef6921d89");
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);
                _HWR_SpeechToText.ProtocolVersion = HttpVersion.Version11;
                _HWR_SpeechToText.Method = "POST";
                _HWR_SpeechToText.ContentType = "audio/x-wav;codec=pcm;bit=16;rate=16000 ";
                _HWR_SpeechToText.Accept = "text/plain";
                _HWR_SpeechToText.Headers.Add("Accept-Topic:Dictation");
                _HWR_SpeechToText.Headers.Add("Accept-Language:en-US");
                _HWR_SpeechToText.SendChunked = true;
                _HWR_SpeechToText.ContentLength = BA_AudioFile.Length;
                Stream stream = _HWR_SpeechToText.GetRequestStream();
                stream.Write(BA_AudioFile, 0, BA_AudioFile.Length);
                //stream.Close();

                HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
                if (HWR_Response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                    Console.WriteLine(SR_Response.ReadToEnd());
                }
                else
                {
                    HWR_Response.GetResponseHeader("x-nuance-sessionid");
                }
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

        //for testing purpose only, accept any dodgy certificate... 
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
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
