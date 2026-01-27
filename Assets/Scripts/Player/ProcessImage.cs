using UnityEngine;
using System.IO;

public class ProcessImage : MonoBehaviour
{
    private const string PiecesFolder = "Assets/Sprites/Background/pieces";
    private const string AlphaFolder = "Assets/Sprites/Background/alpha";
    private const string OutputFolder = "Assets/Masked_Image";

    [ContextMenu("Run Masking for All Files")]
    public void RunForAllFiles()
    {
        // Ensure the output folder exists
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        // Get all .jpg files in the pieces folder
        string[] pieceFiles = Directory.GetFiles(PiecesFolder, "*.jpg");

        foreach (string pieceFile in pieceFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(pieceFile);
            string alphaFile = Path.Combine(AlphaFolder, fileName + ".jpg");

            if (File.Exists(alphaFile))
            {
                // Load the textures
                Texture2D colorTexture = LoadTexture(pieceFile);
                Texture2D maskTexture = LoadTexture(alphaFile);

                if (colorTexture != null && maskTexture != null)
                {
                    // Mask the texture
                    Texture2D result = MaskTexture(colorTexture, maskTexture);

                    // Save the result
                    string outputFilePath = Path.Combine(OutputFolder, fileName + "_MaskedOutput.png");
                    File.WriteAllBytes(outputFilePath, result.EncodeToPNG());

                    Debug.Log($"Processed and saved: {outputFilePath}");
                }
                else
                {
                    Debug.LogError($"Failed to load textures for {fileName}");
                }
            }
            else
            {
                Debug.LogWarning($"No matching alpha file found for {fileName}");
            }
        }
    }

    [ContextMenu("Remove Matching Files")]
    public void RemoveMatchingFiles()
    {
        // Get all .jpg files in the pieces folder
        string[] pieceFiles = Directory.GetFiles(PiecesFolder, "*.jpg");

        foreach (string pieceFile in pieceFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(pieceFile);
            string alphaFile = Path.Combine(AlphaFolder, fileName + ".jpg");

            // Check if a corresponding file exists in the alpha folder
            if (File.Exists(alphaFile))
            {
                // Delete the file from the pieces folder
                File.Delete(pieceFile);
                Debug.Log($"Deleted: {pieceFile}");
            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }

    private Texture2D MaskTexture(Texture2D colorTex, Texture2D maskTex)
    {
        int w = colorTex.width;
        int h = colorTex.height;

        Texture2D output = new Texture2D(w, h, TextureFormat.RGBA32, false);

        Color[] c = colorTex.GetPixels();
        Color[] m = maskTex.GetPixels();
        Color[] o = new Color[c.Length];

        for (int i = 0; i < c.Length; i++)
        {
            float a = m[i].r; // Use grayscale channel as alpha
            o[i] = new Color(c[i].r, c[i].g, c[i].b, c[i].a * a);
        }

        output.SetPixels(o);
        output.Apply();
        return output;
    }
}
