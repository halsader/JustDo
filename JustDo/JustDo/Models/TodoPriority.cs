using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace JustDo.Models {

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum TodoPriority {

        [EnumMember(Value = "not_set")]
        NOT_SET = 0,

        [EnumMember(Value = "low")]
        LOW = 1,

        [EnumMember(Value = "medium")]
        MEDIUM = 2,

        [EnumMember(Value = "high")]
        HIGH = 3
    }
}