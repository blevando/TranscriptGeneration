 
//using PdfSharp.Fonts;
//using System;
//using System.IO;
//using System.Reflection;
//namespace TranscriptGeneration.Common
//{
//    public class CustomFontResolver : IFontResolver
//    {
//        public static readonly CustomFontResolver Instance = new CustomFontResolver();

//        private const string FontName = "Arial";

//        public string DefaultFontName => FontName;

//        public byte[] GetFont(string faceName)
//        {
//            var assembly = Assembly.GetExecutingAssembly();
//            string resource = faceName switch
//            {
//                "Arial#Regular" => "Perfect DOS VGA 437 Win.ttf",
//                "Arial#Bold" => "Perfect DOS VGA 437 Win.ttf",
//                "Arial#Italic" => "Perfect DOS VGA 437 Win.ttf",
//                "Arial#BoldItalic" => "Perfect DOS VGA 437 Win.ttf",
//                _ => throw new ArgumentOutOfRangeException(nameof(faceName), $"Font '{faceName}' not recognized."),
//            };

//            using var stream = assembly.GetManifestResourceStream(resource);
//            if (stream == null) throw new InvalidOperationException($"Could not find embedded resource {resource}");

//            using var ms = new MemoryStream();
//            stream.CopyTo(ms);
//            return ms.ToArray();
//        }

//        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
//        {
//            if (familyName.Equals(FontName, StringComparison.OrdinalIgnoreCase))
//            {
//                if (isBold && isItalic)
//                    return new FontResolverInfo("Arial#BoldItalic");
//                if (isBold)
//                    return new FontResolverInfo("Arial#Bold");
//                if (isItalic)
//                    return new FontResolverInfo("Arial#Italic");
//                return new FontResolverInfo("Arial#Regular");
//            }

//            // Fallback
//            return new FontResolverInfo("Arial#Regular");
//        }
//    }
//}