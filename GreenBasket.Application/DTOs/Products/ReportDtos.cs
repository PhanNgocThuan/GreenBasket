namespace GreenBasket.Application.DTOs.Products
{
    public class LowStockReportItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal StockQty { get; set; }
        public string StockStatus { get; set; } = string.Empty;
    }

    public class RevenueReportItem
    {
        public string PeriodLabel { get; set; } = string.Empty; // "2026-08-08" | "2026-W32" | "2026-08"
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class InventoryTurnoverItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitsSold { get; set; }
        public decimal CurrentStock { get; set; }
        public double TurnoverRatio { get; set; }
    }

    public class BestSellerItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}