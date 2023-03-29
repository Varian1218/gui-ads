using System;
using Ads;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GuiAds
{
    public static class GuiAdUtils
    {
        private struct PopupData
        {
            public string[] Buttons;
            public bool Toggle;
        }

        private class Popup : MonoBehaviour
        {
            private PopupData _data;
            private Action _hidden;
            private LayoutData _layoutData;

            public PopupData Data
            {
                set => _data = value;
            }

            public int Button { get; private set; }
            public bool Earned { get; private set; }
            public Action Hidden
            {
                set => _hidden = value;
            }

            public bool Visible
            {
                set => gameObject.SetActive(value);
            }

            private void Awake()
            {
                _layoutData = new LayoutData
                {
                    Spacing = (0, 10)
                };
            }

            public void Clear()
            {
                Earned = false;
            }

            private void OnGUI()
            {
                (float X, float Y) position = (10, 70);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                const int height = 30;
                if (_data.Toggle)
                {
                    Earned = GUI.Toggle(new Rect(position.X, 70, 50, height), Earned, "Earned");
                    position.Y += height;
                    position.Y += _layoutData.Spacing.Y;
                }

                for (var i = 0; i < _data.Buttons.Length; i++)
                {
                    var p = position;
                    position.Y += height;
                    position.Y += _layoutData.Spacing.Y;
                    if (!GUI.Button(new Rect(p.X, p.Y, 50, height), _data.Buttons[i])) continue;
                    Button = i;
                    gameObject.SetActive(false);
                    _hidden.Invoke();
                }
            }
        }

        private struct LayoutData
        {
            public (float X, float Y) Spacing;
        }

        public static IFullScreenAd CreateRewardedAd()
        {
            var ad = new FullScreenAd();
            var events = new FullScreenAdEvents();
            var popup = new GameObject(nameof(Popup)).AddComponent<Popup>();
            ad.Events = events;
            ad.IsEarned = () => popup.Earned;
            ad.Load = () =>
            {
                popup.Clear();
                popup.Data = new PopupData
                {
                    Buttons = new[]
                    {
                        "Success",
                        "Fail"
                    }
                };
                popup.Hidden = () => events.Loaded?.Invoke(popup.Button);
                popup.Visible = true;
                
            };
            ad.Show = () =>
            {
                popup.Clear();
                popup.Data = new PopupData
                {
                    Buttons = new[] { "Close" },
                    Toggle = true
                };
                popup.Hidden = () => events.Hidden?.Invoke();
                popup.Visible = true;
                events.Showed?.Invoke();
            };
            popup.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(popup);
            return ad;
        }
    }
}