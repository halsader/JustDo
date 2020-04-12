using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    [DataContract]
    [InitOnly]
    public class TodoListEnvelope {

        [DataMember(Name = "todoList")]
        public SortedDictionary<DateTime, IReadOnlyCollection<Todo>> TodoList { get; set; }
    }
}