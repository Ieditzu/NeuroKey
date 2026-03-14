import os
import re

filepath = 'NeuroKey/Assets/Scripts/Editor/TopDownSceneBuilderEditor.cs'
with open(filepath, 'r') as f:
    content = f.read()

# 1. Remove EnsurePlayerSkinController invocation
content = content.replace('EnsurePlayerSkinController(sphere.gameObject);', '')

# 2. Remove EnsurePlayerSkinController method
# The method ends at line 219. We can just use a simple regex since it's a fixed method.
content = re.sub(r'    private static void EnsurePlayerSkinController.*?    }\n\n', '', content, flags=re.DOTALL)

# 3. Remove EnsureLobbySkinSelectionArea invocation
content = content.replace('EnsureLobbySkinSelectionArea(sphere.transform.localScale);', '')

# 4. Remove EnsureLobbySkinSelectionArea method
content = re.sub(r'    private static void EnsureLobbySkinSelectionArea.*?    }\n\n    private static void EnsureTopLeftCustomSkinPipe', '    private static void EnsureTopLeftCustomSkinPipe', content, flags=re.DOTALL)

# 5. Remove CreateSkinSample method
content = re.sub(r'    private static SkinSelectionTrigger CreateSkinSample.*?    }\n\n    private static Transform CreateSelectionTube', '    private static Transform CreateSelectionTube', content, flags=re.DOTALL)

# 6. Change "SphereController" to "BeanController" (already done in other files, but need to redo for this checkout)
content = content.replace('SphereController', 'BeanController')

with open(filepath, 'w') as f:
    f.write(content)
