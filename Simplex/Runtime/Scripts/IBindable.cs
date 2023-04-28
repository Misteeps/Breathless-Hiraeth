using System;

using UnityEngine;


namespace Simplex
{
    #region IBindable
    public interface IBindable
    {
        public IValue IValue { get; set; }
    }
    #endregion IBindable
    #region IBindable <T>
    public interface IBindable<T>
    {
        public IValue<T> IValue { get; set; }
    }
    #endregion IBindable <T>
}