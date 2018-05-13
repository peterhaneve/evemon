namespace EVEMon.Common.Enumerations.CCPAPI
{
    public enum CCPJobCompletedStatus
    {
        Active = 1,
        Paused = 2,
        Ready = 3,

        Delivered = 101,
        Cancelled = 102,
        Reverted = 103,
    }
}