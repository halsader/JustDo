using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace JustDo.Models {
    [DataContract]
    public class TodoFilterCollection {

        [DataMember(Name = "dueDate")]
        public DateRangeFilter DueDate { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "done")]
        public TodoDoneOptions? Done { get; set; } = TodoDoneOptions.DONE;
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum TodoDoneOptions {

        [EnumMember(Value = "done")]
        DONE = 0,

        [EnumMember(Value = "not_done")]
        NOT_DONE = 1,

        [EnumMember(Value = "all")]
        ALL = 3
    }
}