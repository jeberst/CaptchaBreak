using System.Runtime.Serialization;

namespace CloudSpeech.Response
{
    [DataContract]
    public class Hypothesis
    {
        [DataMember(Name = "utterance")]
        public string Utterance { get; set; }
        [DataMember(Name = "confidence")]
        public double Confidence { get; set; }
    }
}
