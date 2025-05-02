using System;

namespace TrackerWebApp.Models
{
    public class MetricCardModel
    {
        public string Title { get; set; }
        public string Icon { get; set; } // e.g., FontAwesome or Bootstrap icon class
        public string Value { get; set; } // Display value, e.g., "34%", "$10K"
        public string Description { get; set; } // Optional subtitle or detail
        public string ColorClass { get; set; } // e.g., "bg-success", "bg-danger"

        public MetricCardModel() { }

        public MetricCardModel(string title, string icon, string value, string description, string colorClass)
        {
            Title = title;
            Icon = icon;
            Value = value;
            Description = description;
            ColorClass = colorClass;
        }
    }
}
