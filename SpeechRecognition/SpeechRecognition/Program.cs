using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;

namespace SpeechRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeechRecognitionEngine engine = new SpeechRecognitionEngine();

            //engine.SetInputToWaveFile("C:\\12108.wav");
            engine.SetInputToDefaultAudioDevice();

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
    }
}
