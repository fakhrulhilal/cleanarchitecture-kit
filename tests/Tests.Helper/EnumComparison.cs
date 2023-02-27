namespace DevKit;

public class EnumComparison
{
    public static IComparer<Enum> ByName => new EnumComparer(true);
    public static IComparer<Enum> ByValue => new EnumComparer(false);

    public class EnumComparer : IComparer<Enum>
    {
        private readonly bool _useName;

        public EnumComparer(bool useName) => _useName = useName;

        public int Compare(Enum? x, Enum? y) {
            if (x is null && y is null) return 0;
            if (x is null && y is not null) return -1;
            if (x is not null && y is null) return 1;

            if (_useName) {
                string left = x?.ToString() ?? string.Empty;
                string right = y?.ToString() ?? string.Empty;
                return left == right ? 0 : 1;
            }
            else {
                long left = Convert.ToInt64(x);
                long right = Convert.ToInt64(y);
                return left == right ? 0 : left < right ? -1 : 1;
            }
        }
    }
}
