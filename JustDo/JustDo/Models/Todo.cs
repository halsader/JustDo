using System;
using System.Runtime.Serialization;
using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    [InitOnly]
    public class Todo {

        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "dueDateUtc")]
        public DateTime DueDateUtc { get; set; }

        [DataMember(Name = "priority")]
        public TodoPriority Priority { get; set; }

        [DataMember(Name = "done")]
        public bool Done { get; set; }
    }
}