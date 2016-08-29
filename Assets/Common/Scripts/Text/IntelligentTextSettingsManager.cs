using System.Collections.Generic;
using UnityEngine;

namespace Common.Text
{
    public class IntelligentTextSettingsManager : MonoBehaviour
    {
        private struct ImageIdHashPredicate
        {
            private readonly IntelligentTextId m_Id;

            public ImageIdHashPredicate(IntelligentTextId i_Id)
            {
                m_Id = i_Id;
            }

            public ImageIdHashPredicate(string i_Id)
            {
                m_Id = new IntelligentTextId(i_Id);
            }

            public bool Match(IntelligentTextAsset i_Other)
            {
                return m_Id == i_Other.Id;
            }
        }

        [SerializeField]
        private TextAsset m_LocalisationFile;
        [SerializeField]
        private TextAsset m_GlobalInsertsFile;

        [SerializeField]
        private Material m_DefaultMaterial;
        [SerializeField]
        private Font m_DefaultFont;

        //[SerializeField]
        //private List<IntelligentTextMaterial> m_Materials;
        [SerializeField]
        private List<IntelligentTextAsset> m_GloabalImages;
        //[SerializeField]
        //private List<IntelligentTextFont> m_GloabalFonts;
        [SerializeField]
        private List<IntelligentTextTransform> m_GlobalTransforms;

        public IntelligentTextAsset assetTest;


        void Awake()
        {
            int imageCountBefore = m_GloabalImages.Count;
            for(int i = imageCountBefore; i >=0; --i)
            {
                IntelligentTextAsset currentItem = m_GloabalImages[i];
                if(string.IsNullOrEmpty(currentItem.FileReference.FilePath) || string.IsNullOrEmpty(currentItem.Id.Name))
                {
                    m_GloabalImages.RemoveAt(i);
                }
            }

            Debug.Assert(imageCountBefore == m_GloabalImages.Count, "Invalid Intelligent Text Settings: Invalid intelligent text image found.");
        }
        


/*

        *recursive insertion pass(localization pass) (have recursion limit)
*parsing pass
            
*inserts
    (no xml parsing)
    *globalInserts
    * localInserts
*images
    * localImages
    *globalImages
* transforms
    (add renderer perceived area for not using the whole image for spacing)
    * globalTransforms
    * localTransforms
        requires:
        * Vector2 size
        * Vector2 perceivedSize
        * Vector2 offset
        * float rotation
        * enum pivot?
        problems:
        * pivoting on bottom/centre/top
        * moving lines of wrapped text if image size pushed the line height further
* interactors
    * localInteractors
    * globalInteractors


structures:
    * textBlock
        * vertices
        * indices
        * uvs
        *...
    * imageBlock
        * vertices
        * indices
        * uvs
        *...
        * */
    }
}
