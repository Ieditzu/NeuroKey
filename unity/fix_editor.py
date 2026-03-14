import os
import re

filepath = 'NeuroKey/Assets/Scripts/Editor/TopDownSceneBuilderEditor.cs'
with open(filepath, 'r') as f:
    content = f.read()

# Comment out the call
content = content.replace('EnsureLobbySkinSelectionArea(sphere.transform.localScale);', '// EnsureLobbySkinSelectionArea(sphere.transform.localScale);')

# Remove CreateSkinSample method and its usages
content = re.sub(r'SkinSelectionTrigger \w+Trigger = CreateSkinSample\(.*?\);', '', content, flags=re.DOTALL)
content = re.sub(r'private static SkinSelectionTrigger CreateSkinSample\(.*?\}\s*\}', '', content, flags=re.DOTALL)
# Wait, let's just use regex to remove the method CreateSkinSample.
# The method ends at line 867 in the previous output.

with open(filepath, 'w') as f:
    f.write(content)
