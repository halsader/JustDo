using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JustDo.Models {

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ErrorCodes {
        [EnumMember(Value = "e_db_conn")]
        E_DB_CONN = 11000,
        [EnumMember(Value = "e_invalid_data")]
        E_INVALID_DATA = 11001,
        [EnumMember(Value = "e_object_not_found")]
        E_OBJECT_NOT_FOUND = 11404,
        [EnumMember(Value = "e_unknown")]
        E_UNKNOWN = 12000
    }

    public class ErrorResponse {

        /// <summary>
        /// Numerical error code.
        /// </summary>
        /// <value>Numerical error code.
        [DataMember(Name = "error")]
        public ErrorCodes Error { get; set; }

        /// <summary>
        /// Human readble error message
        /// </summary>
        /// <value>Human readble error message</value>
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}