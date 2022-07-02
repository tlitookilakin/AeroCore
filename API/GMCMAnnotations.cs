using System;

namespace AeroCore.API
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GMCMSectionAttribute : Attribute
    {
        public string ID { get; set; }
        public GMCMSectionAttribute(string Name)
        {
            ID = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GMCMPageAttribute : Attribute
    {
        public string ID { get; set; }
        public GMCMPageAttribute(string Name)
        {
            ID = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GMCMRangeAttribute : Attribute
    {
        public float? Min { get; set; } = null;
        public float? Max { get; set; } = null;
        public GMCMRangeAttribute(float? min = null, float? max = null)
        {
            Min = min;
            Max = max;
        }
        public GMCMRangeAttribute(int min = 0, int max = 1) : this((float)min, (float)max) { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GMCMParagraphAttribute : Attribute
    {
        public string Content { get; set; }
        public GMCMParagraphAttribute(string text)
        {
            Content = text;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class GMCMImageAttribute : Attribute
    {
        public string Path { get; set; }
        public int Scale { get; set; } = 4;
        public GMCMImageAttribute(string path)
        {
            Path = path;
        }
        public GMCMImageAttribute(string path, int scale) : this(path)
        {
            Scale = scale;
        }
    }
}
