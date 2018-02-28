namespace eShopOnContainers.Droid.Renderers {
    internal static class ActivityExtensions {
        public static void StartActivityForResult<TActivity>([NotNull] this Android.App.Activity packageContext) where TActivity : Android.App.Activity {

            var intent = new Android.Content.Intent(packageContext, typeof(TActivity));
            packageContext.StartActivityForResult(intent, 0);
        }
    }
}