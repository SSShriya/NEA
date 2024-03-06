using System;

public class FileSave
{
    private string filename;
    private int pairingID;
	public FileSave(string name, int id)
	{
        filename = name;
        pairingID = id;
	}

    public void WriteFile()
    {
        //write file to app files            
        string path = Application.Context.GetExternalFilesDir(null).ToString();
        string folderPath = Path.Combine(path, "images", $"{pairingID}");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, $"{filename}");
        File.WriteAllBytes(filePath, imageArray);

        ExifInterface exif = new ExifInterface(filePath);
        exif.SetAttribute(ExifInterface.TagDatetime, DateTime.UtcNow.ToString("yyyy:MM:dd:HH:mm:ss"));
        exif.SetAttribute(ExifInterface.TagArtist, $"{currentID}");
        exif.SaveAttributes();
    }
}
