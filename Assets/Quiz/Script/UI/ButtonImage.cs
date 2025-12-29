using UnityEngine.UIElements;

namespace KanQuiz
{
    [UxmlElement]
    public partial class ButtonImage : Button
    {
        private VisualElement imageElement;
        private Label label;

        [UxmlAttribute("show-image")]
        public bool showImage
        {
            get => imageElement?.style.display == DisplayStyle.Flex;
            set
            {
                if (imageElement == null)
                    return;

                imageElement.style.display = value
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        [UxmlAttribute("label-text")]
        public string labelText
        {
            get => label?.text;
            set
            {
                if (label != null)
                    label.text = value;
            }
        }

        public ButtonImage()
        {
            imageElement = new VisualElement { name = "button-image" };
            imageElement.AddToClassList("button-image");
            hierarchy.Add(imageElement);

            label = new Label { name = "button-label" };
            label.AddToClassList("button-label");
            hierarchy.Add(label);
        }
    }
}
