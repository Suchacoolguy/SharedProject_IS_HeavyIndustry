using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawLengthSet(string description, double weight, string lengths)
    {
        public string Description { get; set; } = description;
        public double Weight { get; set; } = weight;

        public string Lengths
        {
            get => lengths;
            set
            {
                lengths = ListToString(StringToList(value).OrderBy(x => x).ToList());
            }
        }
        public List<int> LengthsAsIntegerList()
        {
            return string.IsNullOrWhiteSpace(lengths) ? [] : StringToList(lengths).Select(value => (int)(value * 1000)).ToList();
        }
        private static string ListToString(List<double> lengths)
        {
            if (lengths.Count == 0)
                return "";
            return string.Join(",", lengths);
        }

        private static List<double> StringToList(string lengths)
        {
            return lengths.Split(',').Select(double.Parse).ToList();
        }
    }
}