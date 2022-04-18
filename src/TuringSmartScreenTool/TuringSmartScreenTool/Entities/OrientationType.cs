namespace TuringSmartScreenTool.Entities
{
    /// <summary>
    /// Screen orientation types.
    /// </summary>
    public enum OrientationType
    {
        /// <summary>
        /// Screen is portrait.
        /// USB-TypeC port is on the right.
        /// </summary>
        Portrait,
        /// <summary>
        /// Screen is portrait.
        /// USB-TypeC port is on the left.
        /// </summary>
        PortraitReverse,
        /// <summary>
        /// Screen is landscape.
        /// USB-TypeC port is on the top.
        /// </summary>
        Landscape,
        /// <summary>
        /// Screen is landscape.
        /// USB-TypeC port is on the bottom.
        /// </summary>
        LandscapeReverse
    }
}
