namespace Osci
{
    public static class GlobalSettings
    {
        /// <summary>
        /// If disabled, no duplicate ID verification is performed within encrypted data.
        /// </summary>
        public static bool IsDuplicateIdCheckEnabled
        {
            get;
            set;
        }

        static GlobalSettings()
        {
            IsDuplicateIdCheckEnabled = true;
        }
    }
}
