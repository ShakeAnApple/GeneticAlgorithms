using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4
{
    static class Extensions
    {
        public static IEnumerable<T> Flatten<T>(this T node, Func<T, IEnumerable<T>> childsSelector)
        {
            yield return node;

            foreach (var child in childsSelector(node))
                foreach (var subitem in child.Flatten(childsSelector))
                    yield return subitem;
        }



        public static string CollectTree<T>(this T node, Func<T, IEnumerable<T>> childsSelector, Func<T, string> nodeFormat)
        {
            var sb = new StringBuilder();
            CollectTreeImpl(sb, string.Empty, string.Empty, node, childsSelector, nodeFormat);
            return sb.ToString();
        }

        private static void CollectTreeImpl<T>(StringBuilder sb, string prefix, string childPrefix, T node, Func<T, IEnumerable<T>> childsSelector, Func<T, string> nodeFormat)
        {
            sb.Append(prefix).Append(" ").Append(nodeFormat(node)).AppendLine();

            var nodeChilds = childsSelector(node).ToArray();
            for (int i = 0; i < nodeChilds.Length; i++)
            {
                var item = nodeChilds[i];

                if (i < nodeChilds.Length - 1)
                    CollectTreeImpl(sb, childPrefix + "  ├─", childPrefix + "  │ ", item, childsSelector, nodeFormat);
                else
                    CollectTreeImpl(sb, childPrefix + "  └─", childPrefix + "    ", item, childsSelector, nodeFormat);
            }

            if (nodeChilds.Length > 0 && !childsSelector(nodeChilds.Last()).Any())
                sb.Append(childPrefix).AppendLine();
        }

    }
}
