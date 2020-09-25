using System;

namespace Spssly.SpssDataset
{
    /// <summary>
    /// Represents a data row or case
    /// </summary>
    public interface IRawRecord
    {
        /// <summary>
        /// The data array. It should only contain strings, doubles or nulls.
        /// </summary>
        object[] Data { get; }

        /// <summary>
        /// Contains each value for this case
        /// </summary>
        /// <param name="index">The 0-based index that corresponds to the variable order</param>
        /// <returns>
        /// An object that can be either a <see cref="String"/> or a<see cref="Double"/>.
        /// When a value was read as SYSMISS it will be <c>null</c>
        /// </returns>
        object this[int index] { get; }

        /// <summary>
        /// Contains each value for this case
        /// </summary>
        /// <param name="variable">The variable to get the value from</param>
        /// <returns>
        /// An object that can be either a <see cref="String"/> or a<see cref="Double"/>.
        /// When a value was read as SYSMISS it will be <c>null</c>
        /// </returns>
        object this[Variable variable] { get; }

        /// <summary>
        /// Gets the proper value of the variable for this record. This method will check the missing values
        /// in case there are, and will return null in case the value is one of them.
        /// Also, if the format fo this variable is a date, it will be tranformed into a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="variable">The variable to get the value from</param>
        /// <returns>
        /// The value for the variable on this record as object, <c>null</c> 
        /// if the value corresponds to one of the custom missing values rules.
        /// </returns>
        object GetValue(Variable variable);
    }
}
