﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using CloudSpeech.Response;
using Newtonsoft.Json;

namespace CloudSpeech
{
    public class SpeechToText
    {
        private string endpointAddress;

        public SpeechToText(string key)
            : this("https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key=" + key, CultureInfo.CurrentCulture)
        {
        }

        public SpeechToText(string endpointAddress, CultureInfo culture)
        {
            this.endpointAddress = endpointAddress + "&lang=" + culture.Name;
        }

        public List<string> Recognize(Stream contentToRecognize)
        {
            RootObject result = null;
            var request = (HttpWebRequest)WebRequest.Create(this.endpointAddress + "&maxresults=6&pfilter=2");
            ConfigureRequest(request);
            var requestStream = request.GetRequestStream();
            ConvertToFlac(contentToRecognize, requestStream);
            var response = request.GetResponse();
            var speechResponses = new List<TextResponse>();
            //using (var responseStream = response.GetResponseStream())
            //{
            //    // output will be something like {"status":0,"id":"2526dd613c874c321bf9abecd5331ed1-1","hypotheses":[{"utterance":"this is a test this is a test","confidence":0.92601025},{"utterance":"this is a test this is the test"},{"utterance":"this is a test this is a text"},{"utterance":"this is a test this is a test txt"},{"utterance":"this is the test this is a test"},{"utterance":"this is a test this is a test hey"}]}
            //    using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
            //    {
            //        var recognizedText = JsonUtil.Deserialise<RecognizedText>(zippedStream);
            //        var a = recognizedText.Hypotheses;
            //        //speechResponses.AddRange(recognizedText.Hypotheses.Select(hypothesis => new TextResponse {Confidence = hypothesis.Confidence, Utterance = hypothesis.Utterance}));
            //        StreamReader SR_Response = new StreamReader(responseStream);
            //        Console.WriteLine(SR_Response.ReadToEnd());
            //    }
            //}

            HttpWebResponse HWR_Response = (HttpWebResponse) request.GetResponse();

            if (HWR_Response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                String responseString = null;

                Char[] read = new Char[1024];
                int count = SR_Response.Read(read, 0, 1024);
                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    responseString = new String(read, 0, count);
                    count = SR_Response.Read(read, 0, 1024);
                }

                result = JsonConvert.DeserializeObject<RootObject>(responseString);
            }
            HWR_Response.Close();

            List<string> googleResults = new List<string>();
            
            foreach(var alternative in result.result.FirstOrDefault().alternative)
            {
                googleResults.Add(alternative.transcript.ToString());
            }
            response.Close();
            return googleResults;
        }

        private static void ConfigureRequest(HttpWebRequest request)
        {
            request.KeepAlive = true;
            //request.SendChunked = true;
            request.ContentType = "audio/x-flac; rate=44100";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
           // request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
           // request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-GB,en-US;q=0.8,en;q=0.6");
            //request.Headers.Set(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
            request.Method = "POST";
        }

        private void ConvertToFlac(Stream sourceStream, Stream destinationStream)
        {
            var audioSource = new WAVReader(null, sourceStream);
            try
            {
                if (audioSource.PCM.SampleRate != 44100)
                {
                    throw new InvalidOperationException("Incorrect frequency - WAV file must be at 44.1 KHz.");
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

    public class JsonUtil
    {
        public static T Deserialise<T>(Stream stream)
        {
            T obj = Activator.CreateInstance<T>();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(stream);
            return obj;
        }
    }


    public class Alternative
    {
        public string transcript { get; set; }
        public double confidence { get; set; }
    }

    public class Result
    {
        public List<Alternative> alternative { get; set; }
        public bool final { get; set; }
    }

    public class RootObject
    {
        public List<Result> result { get; set; }
        public int result_index { get; set; }
    }
}
