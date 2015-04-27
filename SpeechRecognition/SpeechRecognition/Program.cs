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
using NAudio.Wave.SampleProviders;

namespace SpeechRecognition
{
    class Program
    {
        private const string wavFilePath = @"C:\Projects\MS Project\Samples\wavAudioCaptchas";

        private const string filteredFilePath = @"C:\Projects\MS Project\Samples\filteredAudioCaptchas";

        static void Main(string[] args)
        {


            string foldername = "G:\\CaptchaBreak\\Samples\\wavAudioCaptchas\\";
            
            //Josh's Account
            string key = "AIzaSyC7gzoO7E-Gg6yDtn4lhg6wDvD-qQDMXaQ";

            //Kim's Account
            //string key = "AIzaSyCiCvPNxEdOePIdJPA3yIDpzjPbBi1u65c";

            foreach (string file in Directory.EnumerateFiles(foldername, "*.wav"))
            {
                if (!file.Contains("filtered"))
                {
                    //string filename = "G:\\CaptchaBreak\\Samples\\wavAudioCaptchas\\92790.wav";
                    string filename = file;

                    string captcha = filename.Substring(filename.Length - 9, 5);

                    //string captcha = "12108";

                    string filtername = captcha + "filtered.wav";

                    //List<string> googleResults = new List<string>();
                    //googleResults.Add("1 q 10 812");
                    //googleResults.Add("1 q 10 813");
                    //googleResults.Add("1 Q 10 812");
                    //googleResults.Add("1 q 10 814");
                    //googleResults.Add("1210 812");
                    //googleResults.Add("1 q 10 a1c");


                    //Google(filename);
                    List<string> googleResults = GoogleCloud(filename, key);
                    List<string> microsoftResults = MicrosoftSpeech(filename, filtername);
                    List<string> appleResults = Apple(filename, captcha);

                    AnalyzeStrings(googleResults, appleResults, microsoftResults, captcha);
                }

                Thread.Sleep(30000);
            }

        }

        private static List<string> MicrosoftSpeech(string filename, string filtername)
        {
            string filteredPath = filterSound(filename, filtername);
            List<string> microsoftResults = new List<string>();

            Microsoft m = new Microsoft();

            string result = m.Estimate(filteredPath);
            microsoftResults.Add(result);

            return microsoftResults;
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


        private static void StereoToMono(string filename, string outputPath)
        {

            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                //StereoToMonoProvider16 sound = new StereoToMonoProvider16(reader);
                //sound.LeftVolume = 1;
                //ISampleProvider provider = sound.ToSampleProvider();

               // var resampler = new WdlResamplingSampleProvider(provider, 16000);
                

                 var newFormat = new WaveFormat(16000, 16, 1); 
                //using (WaveFileWriter writer = new WaveFileWriter(outputPath, ))
                //{
                //    float[] buffer = new float[16000];
                //    int read = 0;
                //    while ((read = provider.Read(buffer, 0, 16000)) != 0)
                //    {
                //        //writer.Write(buffer, 0, read);
                //        writer.WriteSamples(buffer, 0, read);
                //    }
                //}

                 using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                 {
                     WaveFileWriter.CreateWaveFile(outputPath, conversionStream);
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
                                "https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key=");
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

        private static List<string> Apple(string filename, string captcha)
        {
            try
            {
                List<string> appleResponses = new List<string>();
                string outputPath = "G:\\CaptchaBreak\\Samples\\MonoFiles\\" + captcha + "Mono.wav"; 
                StereoToMono(filename, outputPath);
                FileStream fileStream = File.OpenRead(outputPath);
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
                    Char[] read = new Char[256];
                    int count = SR_Response.Read(read, 0, 256);
                    while (count > 0)
                    {
                        // Dumps the 256 characters on a string and displays the string to the console.
                        String responseString = new String(read, 0, count);
                        var list = responseString.Split('\n');
                        appleResponses = list.ToList<string>();
                        count = SR_Response.Read(read, 0, 256);
                    }

                    Console.WriteLine(SR_Response.ReadToEnd());
                }
                else
                {
                    HWR_Response.GetResponseHeader("x-nuance-sessionid");
                }

                return appleResponses;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }

            Console.ReadLine();
        }

        private static List<string> GoogleCloud(string filename, string key)
        {
            var SpeechToText = new SpeechToText(key);
            List<string> googleResults = null;

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                googleResults = SpeechToText.Recognize(stream);
                var a = 1;
            }
            return googleResults;

        }
 
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


        private static void AnalyzeStrings(List<string> googleResults, List<string> appleResults, List<string>microsoftResults, string captcha)
        {
            bool correct = false;
            TestResult composite = new TestResult();
            int[] Correctness = new int[5];
            composite.correctness = Correctness;


            bool[] correctCompositeArray = new bool[5];

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("Results.txt", true))
            {
                file.WriteLine("Google Results: " + captcha);

                var googleAnalysis = Analyze(googleResults, captcha);
                file.WriteLine("Single Complete Solution: " + googleAnalysis.complete);
                file.WriteLine("Correctness String: ");
                PrintCorrectString(file, googleAnalysis);

                file.WriteLine("Microsoft Results: " + captcha);
                var microsoftAnalysis = Analyze(microsoftResults, captcha);
                file.WriteLine("Single Complete Solution: " + microsoftAnalysis.complete);
                PrintCorrectString(file, microsoftAnalysis);

                file.WriteLine("Apple Results: " + captcha);
                var appleAnalysis = Analyze(appleResults, captcha);
                file.WriteLine("Single Complete Solution: " + appleAnalysis.complete);
                PrintCorrectString(file, appleAnalysis);

                if (googleAnalysis.complete || microsoftAnalysis.complete || appleAnalysis.complete)
                {
                    correct = true;
                }

               
                for (int i = 0; i < captcha.Length; i++)
                {
                    if (googleAnalysis.correctness[i] > 0 || microsoftAnalysis.correctness[i] > 0 || appleAnalysis.correctness[i] > 0)
                    {
                        correctCompositeArray[i] = true;
                        composite.correctness[i] = 1;
                    }

                    if (correctCompositeArray.Contains(false) == false)
                    {
                        composite.complete = true;
                    }
                }

                file.WriteLine("Composite Results: " + captcha);
                file.WriteLine("Composite Complete Solution: " + composite.complete);
                PrintCorrectString(file, composite);              
            }

        }

        private static void PrintCorrectString(System.IO.StreamWriter file, TestResult result)
        {
            for (int i = 0; i < result.correctness.Length; i++)
            {
                file.WriteLine(i + " " + result.correctness[i].ToString());
            }
        }

        private static TestResult Analyze(List<string> Results, string captcha)
        {
            //Analyze Apple
            TestResult Test = new TestResult();

            int[] Correctness = new int[5];
            bool Complete = false;

            for (int i = 0; i < Correctness.Length; i++)
            {
                Correctness[i] = 0;
            }

            foreach (string transcript in Results)
            {
                if (!string.IsNullOrEmpty(transcript))
                {
                    string stripped = transcript.Replace(" ", "");
                    string analyze = stripped;
                    
                    if(stripped.Length >=5)
                    {
                        analyze = stripped.Substring(0, 5);
                    }

                    if (string.Compare(analyze, captcha) == 0)
                    {
                        Complete = true;
                    }

                    int localCount = 0;
                    for (int i = 0; i < analyze.Length; i++)
                    {
                        if (analyze[i] == captcha[i])
                        {
                            localCount++;
                            Correctness[i]++;
                        }
                    }
                }
            }
            Test.correctness = Correctness;
            Test.complete = Complete;
            return Test;

        }
    }

    class TestResult
    {
        public int[] correctness;
        public bool complete = false;
    }
        


    
}
