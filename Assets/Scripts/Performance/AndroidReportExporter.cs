using System;
using System.IO;
using UnityEngine;

public static class AndroidReportExporter
{
    public static void Export(string reportPath, string reportFileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            byte[] reportBytes = File.ReadAllBytes(reportPath);
            AndroidJavaObject activity = GetCurrentActivity();
            AndroidJavaObject reportUri;
            bool copied = CopyToDownloads(reportBytes, reportFileName, activity, out reportUri);

            if (!copied)
            {
                Debug.LogError("[AndroidReportExporter] Could not export the performance report.");
                return;
            }

            if (reportUri == null)
            {
                return;
            }

            AndroidJavaObject shareIntent = new AndroidJavaObject("android.content.Intent", "android.intent.action.SEND");
            shareIntent.Call<AndroidJavaObject>("setType", "text/markdown");
            shareIntent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", reportUri);
            shareIntent.Call<AndroidJavaObject>("addFlags", 1);

            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", shareIntent, "Share performance report");
            activity.Call("startActivity", chooser);
            Debug.Log("[AndroidReportExporter] Report copied to Downloads and Share Sheet opened.");
        }
        catch (Exception exception)
        {
            Debug.LogError("[AndroidReportExporter] Export failed: " + exception.Message);
        }
#else
        Debug.Log("[AndroidReportExporter] Android export skipped outside an Android player.");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject GetCurrentActivity()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    private static bool CopyToDownloads(byte[] reportBytes, string reportFileName, AndroidJavaObject activity, out AndroidJavaObject reportUri)
    {
        reportUri = null;
        using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkVersion = versionClass.GetStatic<int>("SDK_INT");
            if (sdkVersion >= 29)
            {
                reportUri = CopyUsingMediaStore(reportBytes, reportFileName, activity);
                return reportUri != null;
            }
        }

        return CopyUsingLegacyDownloadsPath(reportBytes, reportFileName);
    }

    private static AndroidJavaObject CopyUsingMediaStore(byte[] reportBytes, string reportFileName, AndroidJavaObject activity)
    {
        using (AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues"))
        using (AndroidJavaClass downloadsClass = new AndroidJavaClass("android.provider.MediaStore$Downloads"))
        {
            values.Call("put", "_display_name", reportFileName);
            values.Call("put", "mime_type", "text/markdown");
            values.Call("put", "relative_path", "Download/");
            values.Call("put", "is_pending", 1);

            AndroidJavaObject collection = downloadsClass.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");
            AndroidJavaObject resolver = activity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaObject reportUri = resolver.Call<AndroidJavaObject>("insert", collection, values);
            if (reportUri == null)
            {
                return null;
            }

            using (AndroidJavaObject outputStream = resolver.Call<AndroidJavaObject>("openOutputStream", reportUri))
            {
                outputStream.Call("write", reportBytes);
                outputStream.Call("flush");
            }

            using (AndroidJavaObject completedValues = new AndroidJavaObject("android.content.ContentValues"))
            {
                completedValues.Call("put", "is_pending", 0);
                resolver.Call<int>("update", reportUri, completedValues, null, null);
            }

            Debug.Log("[AndroidReportExporter] Report copied to Downloads URI: " + reportUri.Call<string>("toString"));
            return reportUri;
        }
    }

    private static bool CopyUsingLegacyDownloadsPath(byte[] reportBytes, string reportFileName)
    {
        using (AndroidJavaClass environmentClass = new AndroidJavaClass("android.os.Environment"))
        {
            string downloadsDirectoryName = environmentClass.GetStatic<string>("DIRECTORY_DOWNLOADS");
            AndroidJavaObject downloadsDirectory = environmentClass.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", downloadsDirectoryName);
            string downloadsPath = downloadsDirectory.Call<string>("getAbsolutePath");
            string destinationPath = Path.Combine(downloadsPath, reportFileName);
            File.WriteAllBytes(destinationPath, reportBytes);
            Debug.Log("[AndroidReportExporter] Report copied to " + destinationPath + ". Share Sheet is unavailable on this Android API path.");
            return true;
        }
    }
#endif
}
