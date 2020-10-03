using System;

namespace DotNetCI.Extensions
{
    public static class ICloneableExtensions
    {
        public static T Clone<T>(this T obj)
            where T: ICloneable
        {
            return (T)obj.Clone();
        }

        public static T MutableClone<T>(this T obj, Action<T> mutation)
            where T : ICloneable
        {
            T mutable = obj.Clone<T>();
            mutation?.Invoke(mutable);
            return mutable;
        }

        public static TReciever TransferTo<TReciever, TOwner>(this TOwner owner, Func<TOwner, TReciever>? transfer)
            where TOwner : ICloneable
        {
            if (transfer == null)
            {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
                return default;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            }
            return transfer.Invoke(owner);
        }
    }
}
