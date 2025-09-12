namespace TypesettingMIS.Core.Enums;

public enum OrderStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public enum QuoteStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue
}

public enum EquipmentStatus
{
    Active,
    Inactive,
    Maintenance,
    Retired
}
