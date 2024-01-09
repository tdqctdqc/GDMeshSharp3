// using System.Collections.Generic;
// using System.Linq;
//
// public interface IAttributed
// {
//     IReadOnlyList<IGameAttribute> AttributeList { get; }
// }
//
// public static class IAttributedExt
// {
//     public static TAttribute GetAttribute<TAttribute>(this IAttributed model)
//     {
//         return model.AttributeList
//             .OfType<TAttribute>().First();
//     }
// }