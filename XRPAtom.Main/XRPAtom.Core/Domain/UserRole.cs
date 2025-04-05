namespace XRPAtom.Core.Domain
{
    /// <summary>
    /// Defines the roles available in the XRPAtom system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Standard residential user who can participate in curtailment events
        /// </summary>
        Residential = 1,

        /// <summary>
        /// Energy aggregator or flexibility service provider
        /// </summary>
        Aggregator = 2,

        /// <summary>
        /// Transmission System Operator (TSO) with administrative capabilities
        /// </summary>
        TSO = 3,

        /// <summary>
        /// System administrator with full platform access
        /// </summary>
        Admin = 4,

        /// <summary>
        /// Commercial energy consumer with multiple devices
        /// </summary>
        Commercial = 5,

        /// <summary>
        /// Grid operator with event creation and management capabilities
        /// </summary>
        GridOperator = 6
    }
}