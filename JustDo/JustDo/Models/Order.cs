using System.Runtime.Serialization;

using Newtonsoft.Json;

using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    /// <summary>
    /// Ordering params
    /// </summary>
    [DataContract]
    [InitOnly]
    public class Order {

        /// <summary>
        /// Field to order by
        /// </summary>
        /// <value>Field to order by</value>
        [DataMember(Name = "field")]
        public string Field { get; set; }

        /// <summary>
        /// Ordering direction. Valid values are case-insensitive 'ASC' and 'DESC'. Default is 'DESC' (if ordering direction is not set or differs from valid values)
        /// </summary>
        /// <value>Ordering direction. Valid values are case-insensitive 'ASC' and 'DESC'. Default is 'DESC' (if ordering direction is not set or differs from valid values)</value>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum DirectionEnum {

            /// <summary>
            /// Enum for asc
            /// </summary>
            [EnumMember(Value = "asc")]
            Asc = 0,

            /// <summary>
            /// Enum for desc
            /// </summary>
            [EnumMember(Value = "desc")]
            Desc = 1
        }

        /// <summary>
        /// Ordering direction. Valid values are case-insensitive 'ASC' and 'DESC'. Default is 'DESC' (if ordering direction is not set or differs from valid values)
        /// </summary>
        /// <value>Ordering direction. Valid values are case-insensitive 'ASC' and 'DESC'. Default is 'DESC' (if ordering direction is not set or differs from valid values)</value>
        [DataMember(Name = "direction")]
        public DirectionEnum? Direction { get; set; } = DirectionEnum.Desc;
    }
}