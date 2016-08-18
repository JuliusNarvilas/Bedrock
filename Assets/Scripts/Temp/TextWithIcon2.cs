using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Sources
{
    public class TextWithIcon2 : Text
    {
        public float ImageScalingFactor;

        [System.Serializable]
        public struct IconName
        {
            public string name;
            public Image image;
        }
        public IconName[] inspectorIconList;

        private Image icon;
        private Vector3 iconPosition;
        private Dictionary<Vector3, string> positions = new Dictionary<Vector3, string>();
        private float _fontHeight;
        private float _fontWidth;
        private string fixedString;
        private Dictionary<string, Image> iconList = new Dictionary<string, Image>();
        private Transform imagePrefab;

//#if !UNITY_EDITOR
        /**
        * Unity Inspector cant display Dictionary vars,
        * so we use this little hack to setup the iconList
        */
        new void Start ()
        {
            foreach (IconName icon in inspectorIconList)
            {
                Debug.Log(icon.image);
                iconList.Add(icon.name, icon.image);
            }
            fixedString = this.text;
            this.text = Regex.Replace(this.text, @"\:([a-zA-Z]*):", "$");

            imagePrefab = transform.FindChild("head");
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            Dictionary<int, string> indexes;
            int startVertexIndex;
            int endVertexIndex;
            int j;

            base.OnPopulateMesh(toFill);
            List<UIVertex> vbo = new List<UIVertex>();
            toFill.GetUIVertexStream(vbo);

            positions.Clear();

            indexes = getIndexes(fixedString);
            //icons = GetComponentsInChildren<Image>().ToList();

            foreach (KeyValuePair <int, string> entry in indexes)
            {
                Vector3[] vector3s = new Vector3[6];
                startVertexIndex = entry.Key * 6;
                endVertexIndex = startVertexIndex + 6;

                j = 0;
                for (int i = startVertexIndex; i < endVertexIndex; i++)
                {
                    vector3s[j] = vbo[i].position;
                    j++;
                    if (j == 6) j = 0;
                }

                positions.Add(CenterOfVectors(vector3s), entry.Value);
                _fontHeight = Vector3.Distance(vector3s[0], vector3s[4]);
                _fontWidth = Vector3.Distance(vector3s[0], vector3s[1]);
            }

            foreach (KeyValuePair <Vector3, string> entry in positions)
            {
                GameObject icon = (GameObject)Instantiate(imagePrefab, entry.Key, Quaternion.identity);
                icon.transform.SetParent(transform);
                icon.GetComponent<Image>().sprite = iconList[entry.Value].sprite;
                icon.GetComponent<RectTransform>().anchoredPosition = entry.Key;
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(_fontWidth * ImageScalingFactor, _fontHeight * ImageScalingFactor);
            }

            /*
                for (int i = 0; i < icons.Count; i++)
                {
                    icons[i].rectTransform.anchoredPosition = positions[i];
                    icons[i].rectTransform.sizeDelta = new Vector2(_fontWidth * ImageScalingFactor, _fontHeight * ImageScalingFactor);
                }
            */
        }

        private Vector3 CenterOfVectors(Vector3[] vectors)
        {
            Vector3 sum = Vector3.zero;
            if (vectors == null || vectors.Length == 0)
            {
                return sum;
            }

            foreach (Vector3 vec in vectors)
            {
                sum += vec;
            }
            return sum / vectors.Length;
        }

        private Dictionary<int, string> getIndexes(string text)
        {
            Dictionary<int, string> indexes = new Dictionary<int, string>();
            int diff = 0;

            if (text == null || text.Length == 0)
            {
                return indexes;
            }
            
            foreach (Match match in Regex.Matches(text, @"\:([a-zA-Z]*):"))
            {
                indexes.Add(match.Index - diff, match.Value);
                diff += match.Value.Length - 1; // :ICON: will later be converted to $, so we must fix the offset
            }
            return indexes;
        }
//#endif
    }
}
