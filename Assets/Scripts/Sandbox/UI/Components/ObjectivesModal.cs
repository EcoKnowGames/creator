using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivesModal : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _authorText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _coverImage;

        private Texture2D _coverImageTexture = null;

        public void Init(string title, string author, string description, string coverImageBase64)
        {
            if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(coverImageBase64))
            {
                this.gameObject.SetActive(false);
                return;
            }

            if (_titleText != null)
                _titleText.text = title;

            if (_authorText != null)
                _authorText.text = author;

            if (_descriptionText != null)
                _descriptionText.text = description;

            SetCoverImage(coverImageBase64);

            this.gameObject.SetActive(true);
        }

        private void SetCoverImage(string base64)
        {
            if (_coverImage == null)
            {
                return;
            }

            byte[] imageBytes = Convert.FromBase64String(base64);
            _coverImageTexture = new Texture2D(2, 2);
            if (_coverImageTexture.LoadImage(imageBytes))
            {
                _coverImage.sprite = Sprite.Create(_coverImageTexture, new Rect(0, 0, _coverImageTexture.width, _coverImageTexture.height), Vector2.zero);
                _coverImage.preserveAspect = true;
            }
        }

        public void OnStartPressed()
        {
            //Dismiss
            this.gameObject.SetActive(false);
        }
    }
}
