using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    public class Microsoft
    {
        public string Estimate(string audioCatphaFile)
        {
            SpeechRecognitionEngine engine = new SpeechRecognitionEngine();

            engine.SetInputToWaveFile(audioCatphaFile);

            Choices choices = new Choices();
            choices.Add("1", "2", "3", "4", "5", "6", "7", "8", "9", "0");

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(choices);

            Grammar g = new Grammar(grammarBuilder);
            engine.LoadGrammar(g);

            string result = "";
            engine.SpeechDetected += (sender, evnt) => result = result + "x";
            engine.SpeechRecognized += (sender, evnt) => result = result + evnt.Result.Text;
            engine.SpeechRecognitionRejected += (sender, evnt) => result = result + ReplaceIfClear(evnt.Result.Alternates.Select(x => x.Text).FirstOrDefault(), "-");

            engine.RecognizeAsync(RecognizeMode.Multiple);

            Thread.Sleep(1000);

            return result;
        }

        private string ReplaceIfClear(string check, string replacement)
        {
            if (string.IsNullOrWhiteSpace(check))
            {
                return replacement;
            }

            return check;
        }
    }
}
