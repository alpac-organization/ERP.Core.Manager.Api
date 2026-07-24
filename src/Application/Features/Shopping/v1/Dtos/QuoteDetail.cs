namespace ERP.Core.Manager.Api.Application.Features.Shopping.v1.Dtos;
    public class QuoteDetailDto
    {
        public Guid QuoteDetailId { get; set; }
        public int Amount { get; set; }
        public string? Color { get; set; }
        public decimal IndividualPrice { get; set; }
        public string? Observations { get; set; }
        public string? AssitionalData { get; set; }
        public Guid UnitMeasureId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid QuotationId { get; set; }
    }
