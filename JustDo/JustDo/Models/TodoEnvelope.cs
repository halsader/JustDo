using System.Runtime.Serialization;

using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    [DataContract]
    [InitOnly]
    public class TodoEnvelope {

        [DataMember(Name = "todo")]
        public Todo Todo { get; set; }
    }
}