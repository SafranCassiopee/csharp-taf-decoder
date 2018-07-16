using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_taf_decoder.entity
{
    public class AbstractEntity
    {
        /// <summary>
        ///  an evolution can contain embedded evolutions with different probabilities
        /// </summary>
        public List<Evolution> Evolutions { get; set; } = new List<Evolution>();
    }
}
