using System;

namespace LegacyModels
{
    public class Article
    {
        public int Id { get; set; }
        public string ArticleClass { get; set; }
        public string ArticleRegCode { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public bool HasLots { get; set; }
        
        /// <summary>
        /// Duration in days
        /// </summary>
        public int? LotValidity { get; set; }
        public ArticleExtension Extension { get; set; }
    }
}
