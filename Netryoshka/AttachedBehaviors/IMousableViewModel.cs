namespace Netryoshka
{
    /*
    WARNING: This class is deprecated and should NOT be used.
    Left for historical or instructional value.
    */



    /// <summary>
    /// Interface to be implemented by viewmodels with a view element that has <see cref="MouseOverBehavior"/>
    /// </summary>
    public interface IViewModelWithMouseOverBehavior
    {
        /// <summary>
        /// Gets or sets a value indicating whether the mouse is currently over the associated UI element.
        /// </summary>
        bool IsMouseOver { get; set; }
    }

}
