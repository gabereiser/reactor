using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        private readonly List<string> _names;

        public ScanMultipleFilter(List<string> names)
        {
            _names = names;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current,
            bool errorWhenNoMatch)
        {
            foreach (var c in current)
            {
                var value = c;

                while (true)
                {
                    var container = value as JContainer;

                    value = GetNextScanValue(c, container, value);
                    if (value == null) break;

                    if (value is JProperty property)
                        foreach (var name in _names)
                            if (property.Name == name)
                                yield return property.Value;
                }
            }
        }
    }
}