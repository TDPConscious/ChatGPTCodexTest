using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GameDevUtils
{
    /// <summary>
    /// Node description parsed from Lanhu JSON design export.
    /// Only a subset of fields are supported to keep the tool lightweight.
    /// </summary>
    public sealed class LanhuNode
    {
        /// <summary>Name of the element.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Type of the element, e.g. "group", "image", "text".</summary>
        public string Type { get; init; } = "group";

        /// <summary>Position X in pixels.</summary>
        public float X { get; init; }

        /// <summary>Position Y in pixels.</summary>
        public float Y { get; init; }

        /// <summary>Width in pixels.</summary>
        public float Width { get; init; }

        /// <summary>Height in pixels.</summary>
        public float Height { get; init; }

        /// <summary>Optional image source URL if <see cref="Type"/> is "image".</summary>
        public string? Source { get; init; }

        /// <summary>Optional text content if <see cref="Type"/> is "text".</summary>
        public string? Text { get; init; }

        /// <summary>Child nodes.</summary>
        public List<LanhuNode> Children { get; init; } = new();
    }

    /// <summary>
    /// Parses Lanhu JSON export into a hierarchy of <see cref="LanhuNode"/> objects.
    /// </summary>
    public static class LanhuParser
    {
        /// <summary>
        /// Parses the provided JSON string.
        /// </summary>
        /// <param name="json">JSON text exported from Lanhu.</param>
        /// <returns>The root <see cref="LanhuNode"/>.</returns>
        /// <example>
        /// <code>
        /// string json = File.ReadAllText("design.json");
        /// LanhuNode root = LanhuParser.Parse(json);
        /// </code>
        /// </example>
        public static LanhuNode Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return ParseNode(doc.RootElement);
        }

        private static LanhuNode ParseNode(JsonElement element)
        {
            var node = new LanhuNode
            {
                Name = element.GetProperty("name").GetString() ?? string.Empty,
                Type = element.GetProperty("type").GetString() ?? "group",
                X = element.GetProperty("x").GetSingle(),
                Y = element.GetProperty("y").GetSingle(),
                Width = element.GetProperty("width").GetSingle(),
                Height = element.GetProperty("height").GetSingle(),
                Source = element.TryGetProperty("source", out var src) ? src.GetString() : null,
                Text = element.TryGetProperty("text", out var txt) ? txt.GetString() : null,
            };

            if (element.TryGetProperty("children", out var children))
            {
                foreach (var child in children.EnumerateArray())
                {
                    node.Children.Add(ParseNode(child));
                }
            }

            return node;
        }
    }

#if UNITY_5_3_OR_NEWER
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Builds a Unity UI hierarchy from a <see cref="LanhuNode"/> tree.
    /// The builder aims to match sizes and basic images/text so that minimal manual tweaks are required.
    /// </summary>
    public static class LanhuUnityBuilder
    {
        /// <summary>
        /// Recursively creates Unity UI objects under the specified parent.
        /// </summary>
        /// <param name="node">Root node to create.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The instantiated GameObject.</returns>
        /// <example>
        /// <code>
        /// var root = LanhuParser.Parse(json);
        /// GameObject uiRoot = LanhuUnityBuilder.Build(root, canvasTransform);
        /// </code>
        /// </example>
        public static GameObject Build(LanhuNode node, Transform? parent = null)
        {
            GameObject go;
            switch (node.Type)
            {
                case "image":
                    go = new GameObject(node.Name, typeof(RectTransform), typeof(Image));
                    if (!string.IsNullOrEmpty(node.Source))
                        ApplyImage(go.GetComponent<Image>(), node.Source!);
                    break;
                case "text":
                    go = new GameObject(node.Name, typeof(RectTransform), typeof(Text));
                    go.GetComponent<Text>().text = node.Text ?? string.Empty;
                    break;
                default:
                    go = new GameObject(node.Name, typeof(RectTransform));
                    break;
            }

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(node.Width, node.Height);
            rt.anchoredPosition = new Vector2(node.X, -node.Y);

            if (parent != null)
                rt.SetParent(parent, false);

            foreach (var child in node.Children)
                Build(child, rt);

            return go;
        }

        private static async void ApplyImage(Image image, string url)
        {
            using var uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
            await uwr.SendWebRequest();
            if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            }
        }
    }
#endif
}

