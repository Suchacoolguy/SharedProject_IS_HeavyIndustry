using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawLengthSet
    {
        public string Description { get; set; }
        public double Weight { get; set; }

        private string _lengths;
        public string Lengths
        {
            get => _lengths;
            set
            {
                _lengths = ListToString(StringToList(value).OrderBy(x => x).ToList());
            }
        }

        // 매개변수가 있는 생성자
        public RawLengthSet(string description, double weight, string lengths)
        {
            Description = description;
            Weight = weight;
            Lengths = lengths;
        }

        private string ListToString(List<double> lengths)
        {
            return string.Join(",", lengths);
        }

        private List<double> StringToList(string lengths)
        {
            return lengths.Split(',').Select(double.Parse).ToList();
        }
    }
}