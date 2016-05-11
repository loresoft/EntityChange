using System;

namespace ChangeDetector
{
    public class ChangeRecord
    {
        public string Path { get; set; }

        public ChangeOperation Operation { get; set; }

        public object OrginalValue { get; set; }

        public object CurrentValue { get; set; }
    }

    public enum ChangeOperation
    {
        Add,
        Remove,
        Replace
    }
}