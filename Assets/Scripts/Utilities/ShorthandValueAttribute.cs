using System;

public class ShorthandValueAttribute : Attribute
{
    #region Properties

    /// <summary>
    /// Holds the stringvalue for a value in an enum.
    /// </summary>
    public string ShorthandValue { get; protected set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor used to init a StringValue Attribute
    /// </summary>
    /// <param name="value"></param>
    public ShorthandValueAttribute(string value)
    {
        this.ShorthandValue = value;
    }

    #endregion
}
