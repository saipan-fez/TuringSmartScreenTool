namespace TuringSmartScreenLibrary.Entities
{
    /// <summary>
    /// Screen rotate types.
    /// </summary>
    public enum Rotation
    {
        /// <summary>
        /// No ration.
        /// </summary>
        /// <remarks>
        /// Screen is portrait.
        /// USB-TypeC port is on the right.
        /// </remarks>
        Degree0,
        /// <summary>
        /// Rotate 90-degrees clockwise.
        /// </summary>
        /// <remarks>
        /// Screen is landscape.
        /// USB-TypeC port is on the top.
        /// </remarks>
        Degrees90,
        /// <summary>
        /// Rotate 180-degrees clockwise.
        /// </summary>
        /// <remarks>
        /// Screen is portrait.
        /// USB-TypeC port is on the left.
        /// </remarks>
        Degrees180,
        /// <summary>
        /// Rotate 270-degrees clockwise.
        /// </summary>
        /// <remarks>
        /// Screen is landscape.
        /// USB-TypeC port is on the bottom.
        /// </remarks>
        Degrees270
    }
}
