using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraCyteHomeAssignment.Models
{
    public class ResultsResponse
    {
        public string image_id { get; set; }
        public float intensity_average { get; set; }
        public float focus_score { get; set; }
        public string classification_label { get; set; }
        public int[] histogram { get; set; }
    }
}
