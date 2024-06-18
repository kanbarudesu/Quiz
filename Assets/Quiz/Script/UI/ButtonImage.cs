using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KanQuiz
{
    public class ButtonImage : Button
    {
        public new class UxmlFactory : UxmlFactory<ButtonImage, UxmlTraits> { }

        public new class UxmlTraits : Button.UxmlTraits
        {
            UxmlBoolAttributeDescription showImage = new UxmlBoolAttributeDescription { name = "show-image" };
            UxmlStringAttributeDescription labelText = new UxmlStringAttributeDescription { name = "label-text", defaultValue = "Button" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((ButtonImage)ve).showImage = showImage.GetValueFromBag(bag, cc);
                ((ButtonImage)ve).labelText = labelText.GetValueFromBag(bag, cc);
            }
        }

        private bool m_showImage;
        private string m_labelText;

        public bool showImage
        {
            get => m_showImage;
            set
            {
                m_showImage = value;
                var ve = this.Q(null, "button-image");
                if (ve != null) ve.style.display = m_showImage ? DisplayStyle.Flex : DisplayStyle.None;
                MarkDirtyRepaint();
            }
        }

        public string labelText
        {
            get => m_labelText;
            set
            {
                m_labelText = value;
                Label label = this.Q(null, "button-label") as Label;
                if (label != null) label.text = m_labelText;
                MarkDirtyRepaint();
            }
        }

        public ButtonImage()
        {
            var imageElement = new VisualElement { name = "button-image" };
            imageElement.AddToClassList("button-image");
            Add(imageElement);

            var label = new Label { name = "button-label" };
            label.AddToClassList("button-label");
            Add(label);
        }
    }
}