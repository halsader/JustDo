using System;
using System.Runtime.Serialization;

namespace JustDo.Models {
    [DataContract]
    public class DateRangeFilter {
        [DataMember(Name = "from")]
        public DateTime? From { get; set; }
        [DataMember(Name = "to")]
        public DateTime? To { get; set; }
    }
}