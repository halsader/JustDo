using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    [InitOnly]
    [DataContract]
    public class TodoPagedListEnvelope {

        [DataMember(Name = "todoPaged")]
        public Paged<SortedDictionary<DateTime, IReadOnlyCollection<Todo>>> TodoPaged { get; set; }
    }
}