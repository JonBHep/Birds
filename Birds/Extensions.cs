namespace Birds;

public static class Extensions
{
    // Uri.GetBitmapImage
    public static System.Windows.Media.Imaging.BitmapImage GetBitmapImage(
        this System.Uri imageAbsolutePath,
        System.Windows.Media.Imaging.BitmapCacheOption bitmapCacheOption = System.Windows.Media.Imaging.BitmapCacheOption.Default)
    {
        System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
        image.BeginInit();
        image.CacheOption = bitmapCacheOption;
        image.UriSource = imageAbsolutePath;
        image.EndInit();

        return image;
    }
}