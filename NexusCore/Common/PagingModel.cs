using System;
namespace NexusCore.Common
{
	public class PagingModel
	{
        public string Search { get; set; }
        public int CurrentPage { get; set; }
        public int Records { get; set; }
        internal int Skip { get => (CurrentPage - 1) * Records; } 

        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
    }
}

