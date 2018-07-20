using System.Collections.Generic;

namespace csharp_taf_decoder.entity
{
    public class AbstractEntity
    {
        /// <summary>
        /// An evolution can contain embedded evolutions with different probabilities
        /// </summary>
        public List<Evolution> Evolutions { get; set; } = new List<Evolution>();
    }
}
