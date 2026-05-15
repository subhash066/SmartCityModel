using UnityEngine;
using UnityEngine.TextCore.Text;

public static class FontAssetPreWarm
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PreWarmFontAssets()
    {
        FontAsset[] fonts = Resources.FindObjectsOfTypeAll<FontAsset>();
        foreach (var font in fonts)
        {
            if (font != null)
            {
                font.HasCharacter('A');
            }
        }
    }
}
