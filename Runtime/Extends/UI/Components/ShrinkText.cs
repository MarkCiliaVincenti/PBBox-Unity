/*--------------------------------------------------------
 *Copyright (c) 2022 PlusBrackets
 *@update: 2022.05.21
 *@author: PlusBrackets
 --------------------------------------------------------*/
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PBBox;

namespace PBBox.UI
{
    [AddComponentMenu("PBBox/UI/Components/Shrink Text")]
    public class ShrinkText : Text
    {
        /// <summary>
        /// 当前可见的文字行数
        /// </summary>
        public int VisibleLines { get; private set; }
        private readonly UIVertex[] _tmpVerts = new UIVertex[4];

        private void _UseFitSettings()
        {
            TextGenerationSettings settings = GetGenerationSettings(rectTransform.rect.size);
            settings.resizeTextForBestFit = false;

            if (!resizeTextForBestFit)
            {
                cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
                return;
            }

            int minSize = resizeTextMinSize;
            int txtLen = text.Length;
            for (int i = resizeTextMaxSize; i >= minSize; --i)
            {
                settings.fontSize = i;
                cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
                if (cachedTextGenerator.characterCountVisible == txtLen) break;
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (null == font) return;

            m_DisableFontTextureRebuiltCallback = true;
            _UseFitSettings();

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            int vertCount = verts.Count;

            // We have no verts to process just return (case 1037923)
            if (vertCount <= 0)
            {
                toFill.Clear();
                return;
            }

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    _tmpVerts[tempVertsIndex] = verts[i];
                    _tmpVerts[tempVertsIndex].position *= unitsPerPixel;
                    _tmpVerts[tempVertsIndex].position.x += roundingOffset.x;
                    _tmpVerts[tempVertsIndex].position.y += roundingOffset.y;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(_tmpVerts);
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    _tmpVerts[tempVertsIndex] = verts[i];
                    _tmpVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(_tmpVerts);
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
            VisibleLines = cachedTextGenerator.lineCount;
        }
    }
}