# ChatGPTCodexTest

Utilities and helpers for game development.

## Lanhu UI Builder

Parse Lanhu (蓝湖) JSON design exports and generate a matching Unity UI hierarchy. Large layout and sliced images are created automatically so only minimal manual tweaks are required.

```csharp
string json = File.ReadAllText("design.json");
LanhuNode root = LanhuParser.Parse(json);
LanhuUnityBuilder.Build(root, canvasTransform);
```

The actual Unity instantiation requires compiling within Unity (`UNITY_5_3_OR_NEWER`).

