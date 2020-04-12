using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Models {
    /// <summary>
    /// Paged Data
    /// </summary>
    [DataContract]
    [InitOnly]
    public class Paged<T> {

        /// <summary>
        /// Total items count
        /// </summary>
        /// <value>Total items count</value>
        [DataMember(Name = "totalItems")]
        public long? TotalItems { get; set; }

        /// <summary>
        /// Items collection (with items count less or equal ItemsPerPage)
        /// </summary>
        /// <value>Items collection (with items count less or equal ItemsPerPage)</value>
        [DataMember(Name = "items")]
        public T Items { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        /// <value>Current page number</value>
        [DataMember(Name = "pageNum")]
        public int? PageNum { get; set; }

        /// <summary>
        /// Count of items per one page
        /// </summary>
        /// <value>Count of items per one page</value>
        [DataMember(Name = "itemsPerPage")]
        public int? ItemsPerPage { get; set; }
    }
}