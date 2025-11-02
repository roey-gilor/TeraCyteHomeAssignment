using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TeraCyteHomeAssignment.Models
{
    public class HistoryItem
    {
        public BitmapImage Thumbnail { get; set; }
        public string Classification { get; set; }
        public double IntensityAvg { get; set; }
        public double FocusScore { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
