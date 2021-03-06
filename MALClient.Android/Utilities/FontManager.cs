using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;

namespace MALClient.Android
{
    public class FontManager
    {
        public const string TypefacePath =  "fonts/fontawesome-webfont.ttf";

        private static readonly Dictionary<string, Typeface> CacheTypefaces =new Dictionary<string, Typeface>();

        public static Typeface GetTypeface(Context context, string font)
        {
            if (CacheTypefaces.ContainsKey(font))
                return CacheTypefaces[font];

            var typeFace =  Typeface.CreateFromAsset(context.Assets, font);

            CacheTypefaces.Add(font,typeFace);

            return typeFace;
        }

    }
}