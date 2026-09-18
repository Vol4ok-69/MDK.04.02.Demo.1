using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp.Helpers;

public static class ImageHelper
{
    public static ImageSource GetImage(string? photo)
    {
        string? imagePath = GetImagePath(photo);

        if (!string.IsNullOrWhiteSpace(imagePath) &&
            File.Exists(imagePath))
        {
            try
            {
                return LoadFromFile(imagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        string? packPath = GetPackResourcePath(photo);

        if (!string.IsNullOrWhiteSpace(packPath))
        {
            try
            {
                return LoadPackResource(packPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        return LoadPackResource("Resources/picture.png");
    }

    public static string? GetImagePath(string? photo)
    {
        if (!TryNormalizePhoto(photo, out string normalized))
            return null;

        string baseDirectory = AppContext.BaseDirectory;

        string[] parts =
            normalized.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2 &&
            parts[0].Equals(
                "Images",
                StringComparison.OrdinalIgnoreCase))
        {
            if (TryResolvePath(
                    baseDirectory,
                    normalized,
                    out string imagePath) &&
                File.Exists(imagePath))
            {
                return imagePath;
            }

            return null;
        }

        if (parts.Length == 2 &&
            parts[0].Equals(
                "Resources",
                StringComparison.OrdinalIgnoreCase))
        {
            if (TryResolvePath(
                    baseDirectory,
                    normalized,
                    out string resourcePath) &&
                File.Exists(resourcePath))
            {
                return resourcePath;
            }

            return null;
        }

        if (parts.Length == 1)
        {
            if (TryResolvePath(
                    Path.Combine(baseDirectory, "Resources"),
                    normalized,
                    out string resourcePath) &&
                File.Exists(resourcePath))
            {
                return resourcePath;
            }

            if (TryResolvePath(
                    baseDirectory,
                    normalized,
                    out string rootPath) &&
                File.Exists(rootPath))
            {
                return rootPath;
            }
        }

        return null;
    }

    public static bool TryGetManagedImagePath(
        string? photo,
        out string path)
    {
        path = string.Empty;

        if (!TryNormalizePhoto(
                photo,
                out string normalized))
        {
            return false;
        }

        string[] parts =
            normalized.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 ||
            !parts[0].Equals(
                "Images",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return TryResolvePath(
            AppContext.BaseDirectory,
            normalized,
            out path);
    }

    public static bool DeleteManagedImage(string? photo)
    {
        if (!TryGetManagedImagePath(
                photo,
                out string path))
        {
            return false;
        }

        try
        {
            if (File.Exists(path))
                File.Delete(path);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    private static ImageSource LoadFromFile(
        string path)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource =
            new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        return bitmap;
    }

    private static ImageSource LoadPackResource(
        string resourcePath)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource =
            new Uri(
                $"pack://application:,,,/{resourcePath}",
                UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        return bitmap;
    }

    private static string? GetPackResourcePath(
        string? photo)
    {
        if (!TryNormalizePhoto(
                photo,
                out string normalized))
        {
            return null;
        }

        string[] parts =
            normalized.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
            return normalized;

        return $"Resources/{normalized}";
    }

    private static bool TryNormalizePhoto(
        string? photo,
        out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(photo))
            return false;

        string value =
            photo.Trim().Replace('\\', '/');

        if (value.StartsWith('/') ||
            value.Contains("://") ||
            Path.IsPathRooted(value))
        {
            return false;
        }

        string[] parts =
            value.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length is < 1 or > 2)
            return false;

        foreach (string part in parts)
        {
            if (part == "." || part == "..")
                return false;

            if (part.IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }
        }

        if (parts.Length == 2)
        {
            bool validRoot =
                parts[0].Equals(
                    "Images",
                    StringComparison.OrdinalIgnoreCase)
                ||
                parts[0].Equals(
                    "Resources",
                    StringComparison.OrdinalIgnoreCase);

            if (!validRoot)
                return false;
        }

        normalized =
            string.Join("/", parts);

        return true;
    }

    private static bool TryResolvePath(
        string rootDirectory,
        string relativePath,
        out string fullPath)
    {
        fullPath = string.Empty;

        string root =
            Path.GetFullPath(rootDirectory)
                .TrimEnd(
                    Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        string candidate;

        try
        {
            candidate =
                Path.GetFullPath(
                    Path.Combine(
                        rootDirectory,
                        relativePath));
        }
        catch
        {
            return false;
        }

        if (!candidate.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }
}