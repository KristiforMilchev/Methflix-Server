using System;
using System.Collections;
using System.Text;
using QRCoder;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

public class CodeGenerator
{
    public static string GenerateQrCodeAsciiArt(string text, int widthScale = 4, int heightScale = 2)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);

        int moduleCount = qrCodeData.ModuleMatrix.Count;
        var moduleData = qrCodeData.ModuleMatrix;

        string qrCodeAsciiArt = "";

        for (int y = 0; y < moduleCount; y++)
        {
            for (int sy = 0; sy < heightScale; sy++)
            {
                for (int x = 0; x < moduleCount; x++)
                {
                    bool isBlack = moduleData[x][y];

                    // Repeat the black or white module based on the widthScale factor
                    for (int sx = 0; sx < widthScale; sx++)
                    {
                        char moduleChar = isBlack ? '█' : ' ';
                        qrCodeAsciiArt += moduleChar;
                    }
                }
                qrCodeAsciiArt += "\n";
            }
        }

        return qrCodeAsciiArt;
    }


}
 