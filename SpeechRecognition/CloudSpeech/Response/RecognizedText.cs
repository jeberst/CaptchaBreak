using System.Runtime.Serialization;

namespace CloudSpeech.Response
{
    [DataContract]
    public class RecognizedText
    {
        [DataMember(Name = "hypotheses")]
        public Hypothesis[] Hypotheses { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
